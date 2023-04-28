using System.Reactive.Linq;
using System.Reactive.Concurrency;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.KSql.Query.PushQueries;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq;

public class QbservableExtensionsTests : Infrastructure.IntegrationTests
{
  protected static string StreamName = "tweetsTest";
  private static readonly string TopicName = "tweetsTestTopic";

  private static Tweet Tweet1 => TweetsProvider.Tweet1;

  private static Tweet Tweet2 => TweetsProvider.Tweet2;

  [OneTimeSetUp]
  public static async Task ClassInitialize()
  {
    await InitializeDatabase();
  }

  private static TweetsProvider tweetsProvider = null!;
  private static readonly string SingleLadiesStreamName = "singleLadies";

  protected static async Task InitializeDatabase()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();

    tweetsProvider = new TweetsProvider(RestApiProvider);
    var result = await tweetsProvider.CreateTweetsStream(StreamName, TopicName);
    result.Should().BeTrue();

    result = await tweetsProvider.InsertTweetAsync(Tweet1, StreamName);

    result.Should().BeTrue();

    result = await tweetsProvider.InsertTweetAsync(Tweet2, StreamName);

    result.Should().BeTrue();

    var entityCreationMetadata = new EntityCreationMetadata(SingleLadiesStreamName, 1);
    var response = await RestApiProvider.CreateStreamAsync<SingleLady>(entityCreationMetadata, true);
    response = await RestApiProvider.InsertIntoAsync(new SingleLady { Name = "E.T."}, new InsertProperties());
  }

  [OneTimeTearDown]
  public static async Task ClassCleanup()
  {
    var result = await RestApiProvider.DropStreamAndTopic(StreamName);
  }

  protected virtual ksqlDB.RestApi.Client.KSql.Linq.IQbservable<Tweet> QuerySource =>
    Context.CreateQueryStream<Tweet>(StreamName);

  [Test]
  public async Task Select()
  {
    //Arrange
    int expectedItemsCount = 2;
      
    var source = QuerySource
      .ToAsyncEnumerable();
      
    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var expectedValues = new List<Tweet>
    {
      Tweet1, Tweet2
    };
      
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    CollectionAssert.AreEqual(expectedValues, actualValues);
  }

  [Test]
  public async Task Take()
  {
    //Arrange
    int expectedItemsCount = 1;
      
    var source = QuerySource
      .Take(expectedItemsCount)
      .ToAsyncEnumerable();
      
    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var expectedValues = new List<Tweet>
    {
      Tweet1
    };

    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    CollectionAssert.AreEqual(expectedValues, actualValues);
  }

  [Test]
  public async Task Where_MessageWasFiltered()
  {
    //Arrange
    int expectedItemsCount = 1;
      
    var source = QuerySource
      .Where(p => p.Message != "Hello world")
      .ToAsyncEnumerable();
      
    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    Assert.AreEqual(actualValues[0].Message, Tweet2.Message);
  }

  [Test]
  public async Task Between_MessageWasNotFiltered()
  {
    //Arrange
    int expectedItemsCount = 1;
      
    var source = QuerySource
      .Where(p => p.Amount.Between(1, 100))
      .ToAsyncEnumerable();
      
    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    Assert.AreEqual(actualValues[0].Amount, Tweet2.Amount);
  }

  [Test]
  public async Task Subscribe()
  {
    //Arrange
    var semaphore = new SemaphoreSlim(initialCount: 0, 1);
    var actualValues = new List<Tweet>();

    int expectedItemsCount = 2;
      
    var source = QuerySource;

    //Act
    using var subscription = source.Take(expectedItemsCount).Subscribe(c => actualValues.Add(c), e => semaphore.Release(), () => semaphore.Release());
    await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
  }

  [Test]
  public async Task SubscribeAsync_ReturnQueryId()
  {
    //Arrange
    var semaphore = new SemaphoreSlim(initialCount: 0, 1);
    var actualValues = new List<Tweet>();

    int expectedItemsCount = 2;
      
    var source = QuerySource;

    //Act
    var subscription = await source.Take(expectedItemsCount).SubscribeAsync(c => actualValues.Add(c), e => semaphore.Release(), () => semaphore.Release());
    await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    subscription.QueryId.Should().NotBeNullOrEmpty();
  }

  [Test]
  public void SubscribeAsync_UnknownTopic()
  {
    //Arrange
    var source = Context.CreateQueryStream<Tweet>(StreamName+"xyz");

    Assert.ThrowsAsync<KSqlQueryException>(() =>
    {
      //Act
      var subscription = source.SubscribeAsync(c => { }, e => { }, () => { });

      return subscription;
    });
  }

  [Test]
  public async Task SubscribeAsync_UnknownTopic_NullQueryId()
  {
    //Arrange
    var source = Context.CreateQueryStream<Tweet>(StreamName+"xyz");

    Subscription? subscription = null;

    //Act
    try
    {
      subscription = await source.SubscribeAsync(c => { }, e => { }, () => {});
    }
    catch (Exception)
    {
      //Assert
      subscription?.QueryId.Should().BeNull();
    }
  }

  [Test]
  public async Task SubscribeAsync_Canceled()
  {
    //Arrange
    var semaphore = new SemaphoreSlim(initialCount: 0, 1);
    var actualValues = new List<Tweet>();

    var source = QuerySource;

    var cts = new CancellationTokenSource();

    //Act
    var subscription = await source.SubscribeOn(ThreadPoolScheduler.Instance)
      .SubscribeAsync(c => actualValues.Add(c), e => semaphore.Release(), () => semaphore.Release(), cancellationToken: cts.Token);
      
    cts.Cancel();
    await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

    //Assert
    Assert.AreEqual(0, actualValues.Count);
    subscription.QueryId.Should().NotBeNullOrEmpty();
  }

  [Test]
  [Ignore("TODO")]
  public async Task SubscribeOn_Blocks()
  {
    //Arrange
    var actualValues = new List<Tweet>();

    int expectedItemsCount = 2;
      
    var source = QuerySource;

    //Act
    var subscription = await source.Take(expectedItemsCount)
      .SubscribeOn(ImmediateScheduler.Instance)
      .SubscribeAsync(c => actualValues.Add(c), e => {}, () => {});

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
  }

  [Test]
  public async Task ObserveOn_TaskPoolScheduler_ReceivesValuesOnNewThread()
  {
    //Arrange
    var semaphore = new SemaphoreSlim(initialCount: 0, 1);
    var source = QuerySource;

    var currentThread = Thread.CurrentThread.ManagedThreadId;
    int? observeOnThread = null;

    //Act
    var subscription = await source.Take(1)
      .ObserveOn(TaskPoolScheduler.Default)
      .SubscribeAsync(_ => observeOnThread = Thread.CurrentThread.ManagedThreadId, e => semaphore.Release(), () => semaphore.Release());
      
    await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

    //Assert
    observeOnThread.Should().NotBeNull();
    Assert.AreNotEqual(currentThread, observeOnThread!.Value);
  }

  [Test]
  public async Task ObserveOn_TaskPoolScheduler_ReceivesValuesOnNonThreadPoolThread()
  {
    //Arrange
    var semaphore = new SemaphoreSlim(initialCount: 0, 1);
    var source = QuerySource;

    Thread observeOnThread = null!;

    //Act
    var subscription = source.Take(1)
      .ObserveOn(TaskPoolScheduler.Default)
      .Subscribe(_ => observeOnThread = Thread.CurrentThread, e => semaphore.Release(), () => semaphore.Release());

    await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

    //Assert
    observeOnThread.Should().NotBeNull();
    observeOnThread.IsThreadPoolThread.Should().BeFalse();

    using(subscription){}
  }

  [Test]
  public async Task ToObservable()
  {
    //Arrange
    var semaphore = new SemaphoreSlim(initialCount: 0, 1);
    var actualValues = new List<Tweet>();

    int expectedItemsCount = 2;
      
    var source = QuerySource
      .ToObservable();

    //Act
    using var subscription = source.Take(expectedItemsCount).Subscribe(c => actualValues.Add(c), e => semaphore.Release(), () => semaphore.Release());
    await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
  }

  [Test]
  public async Task GroupBy()
  {
    //Arrange
    int expectedItemsCount = 2;

    var source = QuerySource
      .GroupBy(c => c.Id)
      .Select(g => new {Id = g.Key, Count = g.Count(c => c.Message)})
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    Assert.AreEqual(1, actualValues[0].Count);
    Assert.AreEqual(Tweet1.Id, actualValues[0].Id);

    Assert.AreEqual(1, actualValues[1].Count);
    Assert.AreEqual(Tweet2.Id, actualValues[1].Id);
  }

  [Test]
  public async Task Having()
  {
    //Arrange
    int expectedItemsCount = 2;

    var source = QuerySource
      .GroupBy(c => c.Id)
      .Having(c => c.Count(g => g.Message) == 1)
      .Select(g => new {Id = g.Key, Count = g.Count(c => c.Message)})
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    Assert.AreEqual(1, actualValues[0].Count);
    Assert.AreEqual(Tweet1.Id, actualValues[0].Id);

    Assert.AreEqual(1, actualValues[1].Count);
    Assert.AreEqual(Tweet2.Id, actualValues[1].Id);
  }

  [Test]
  public async Task WindowedBy()
  {
    //Arrange
    int expectedItemsCount = 2;

    var source = QuerySource
      .GroupBy(c => c.Id)
      .WindowedBy(new TimeWindows(Duration.OfMilliseconds(100)))
      .Select(g => new {Id = g.Key, Count = g.Count(c => c.Message)})
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);

    Assert.AreEqual(1, actualValues[0].Count);
    Assert.AreEqual(Tweet1.Id, actualValues[0].Id);

    Assert.AreEqual(1, actualValues[1].Count);
    Assert.AreEqual(Tweet2.Id, actualValues[1].Id);
  }

  [Test]
  public async Task WindowedBy_WithFinalOutputRefinement()
  {
    //Arrange
    var source = QuerySource
        .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
        .GroupBy(c => c.Id)
        .WindowedBy(
          new TimeWindows(Duration.OfSeconds(2), OutputRefinement.Final).WithGracePeriod(Duration.OfSeconds(2)))
        .Select(g => new {Id = g.Key, Count = g.Count(c => c.Message)});

    var semaphore = new SemaphoreSlim(0, 1);
    var actualValues = new List<int>();

    //Act
    source.Subscribe(c => actualValues.Add(c.Id), e => semaphore.Release(), () => semaphore.Release());
    await Task.Delay(TimeSpan.FromSeconds(3));

    var result = await tweetsProvider.InsertTweetAsync(Tweet2, StreamName);

    //Assert
    await semaphore.WaitAsync(TimeSpan.FromSeconds(6));
    actualValues.Count.Should().BeGreaterOrEqualTo(1);
  }
    
  [Test]
  public async Task QueryRawKSql()
  {
    //Arrange
    int expectedItemsCount = 2;
      
    string ksql = @"SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

    QueryParameters queryParameters = new QueryParameters
    {
      Sql = ksql,
      [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var source = Context.CreateQuery<Tweet>(queryParameters);
      
    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
  }
    
  [Test]
  public async Task QueryStreamRawKSql()
  {
    //Arrange
    int expectedItemsCount = 2;
      
    string ksql = @"SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var source = Context.CreateQueryStream<Tweet>(queryStreamParameters);
      
    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
  }

  [Test]
  public async Task InClauseFilter()
  {
    //Arrange
    int expectedItemsCount = 1;

    var orderTypes = new List<int> { 1, 3 };

    var source = QuerySource
      .Where(c => orderTypes.Contains(c.Id))
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    Assert.AreEqual(actualValues[0].Id, 1);
  }

  [Test]
  public async Task ListContainsProjection()
  {
    //Arrange
    int expectedItemsCount = 1;

    var orderTypes = new List<int> { 1, 3 };

    var c = QuerySource
      .Select(c => orderTypes.Contains(c.Id)).ToQueryString();

    var source = QuerySource
      .Select(c => orderTypes.Contains(c.Id))
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Should().BeTrue();
  }

  [Test]
  public async Task WithOffsetResetPolicy()
  {
    //Arrange
    var semaphoreSlim = new SemaphoreSlim(0, 1);
    var actualValues = new List<Tweet>();

    using var subscription = QuerySource.WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Take(1)
      .ToObservable()
      .Timeout(TimeSpan.FromSeconds(40))
      .Subscribe(c =>
      {
        actualValues.Add(c);
      }, e => { semaphoreSlim.Release(); }, () => { semaphoreSlim.Release(); });   

    Tweet tweet3 = new()
    {
      Id = 3,
      Message = "42"
    };

    await Task.Delay(2000);

    //Act
    await tweetsProvider.InsertTweetAsync(tweet3, StreamName);
      
    //Assert
    await semaphoreSlim.WaitAsync();
    var expectedValues = new List<Tweet>
    {
      tweet3
    };
      
    CollectionAssert.AreEqual(expectedValues, actualValues);
  }
    
  [Test]
  public async Task ExplainAsync()
  {
    //Arrange
    var query = QuerySource.Where(c => c.Message == "ET");

    //Act
    var description = await query.ExplainAsync();

    //Assert
    description[0].StatementText.Should().Be(@"EXPLAIN SELECT * FROM tweetsTest
WHERE MESSAGE = 'ET' EMIT CHANGES;");
    description[0].QueryDescription.QueryType.Should().Be("PUSH");
    description[0].QueryDescription.ExecutionPlan.Should().NotBeNullOrEmpty();
  }

  [Test]
  public async Task ExplainAsStringAsync()
  {
    //Arrange

    //Act
    var description = await QuerySource.ExplainAsStringAsync();

    //Assert
    description.Should().Contain("EXPLAIN SELECT * FROM tweetsTest EMIT CHANGES;");
  }

  [Test]
  public async Task SinglePropertySelector()
  {
    //Arrange
    int expectedItemsCount = 1;
    string name = "E.T.";

    var source = QuerySource
      .Select(c => name)
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Should().Be(name);
  }

  private record SingleLady
  {
    public string Name { get; init; } = null!;
  }

  [Test]
  public async Task SinglePropertyInstanceSelector()
  {
    //Arrange
    int expectedItemsCount = 1;

    var source = Context.CreateQueryStream<SingleLady>(SingleLadiesStreamName)
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Name.Should().Be("E.T.");
  }

  [Test]
  public async Task SingleStructPropertySelector()
  {
    //Arrange
    int expectedItemsCount = 1;
    int year = 2022;

    var source = QuerySource
      .Select(c => new DateTimeOffset(new DateTime(year, 9, 23), TimeSpan.FromHours(2)))
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Year.Should().Be(year);
  }

  [Test]
  public async Task SelectAsInt()
  {
    //Arrange
    int expectedItemsCount = 1;

    var ksql =
      $"SELECT 42 FROM {StreamName} EMIT CHANGES LIMIT 1;";

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var source = Context.CreateQueryStream<int>(queryStreamParameters);

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues[0].Should().Be(42);
  }

  [Test]
  public async Task SelectAsArray()
  {
    //Arrange
    int expectedItemsCount = 1;

    var ksql =
      $"SELECT ARRAY[1, 2] FROM {StreamName} EMIT CHANGES LIMIT 1;";

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var source = Context.CreateQueryStream<int[]>(queryStreamParameters);

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    CollectionAssert.AreEqual(new[]{1,2}, actualValues[0]);
  }

  private record MyStruct
  {
    public string Name { get; set; } = null!;
  }

  [Test]
  public async Task SelectAsStruct()
  {
    //Arrange
    int expectedItemsCount = 1;

    var ksql =
      $"SELECT STRUCT(NAME := 'E.T.') FROM {StreamName} EMIT CHANGES LIMIT 1;";

    QueryStreamParameters queryStreamParameters = new QueryStreamParameters
    {
      Sql = ksql,
      [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
    };

    var source = Context.CreateQueryStream<MyStruct>(queryStreamParameters);

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues[0].Name.Should().Be("E.T.");
  }
}
