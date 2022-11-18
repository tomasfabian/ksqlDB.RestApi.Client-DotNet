using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq;

[TestClass]
public class AggregationTests : Infrastructure.IntegrationTests
{
  private static MoviesProvider moviesProvider;
  private static TweetsProvider tweetsProvider;

  private static readonly string tweetsTopicName = "tweetsTestTopic";

  [ClassInitialize]
  public static async Task ClassInitialize(TestContext context)
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();

    moviesProvider = new MoviesProvider(RestApiProvider);

    await moviesProvider.CreateTablesAsync();

    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie2);

    tweetsProvider = new TweetsProvider(RestApiProvider);

    await tweetsProvider.CreateTweetsStream(TweetsStreamName, tweetsTopicName);

    await tweetsProvider.InsertTweetAsync(TweetsProvider.Tweet1, TweetsStreamName);
    await tweetsProvider.InsertTweetAsync(TweetsProvider.Tweet2, TweetsStreamName);
  }

  [ClassCleanup]
  public static async Task ClassCleanup()
  {
    await moviesProvider.DropTablesAsync();

    moviesProvider = null;
    tweetsProvider = null;
  }

  [TestMethod]
  public async Task Histogram()
  {
    await TestHistogram(Context.CreateQueryStream<Movie>(MoviesProvider.MoviesTableName));
  }

  private async Task TestHistogram(IQbservable<Movie> querySource)
  {
    //Arrange
    int expectedItemsCount = 2;

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new {Id = l.Key, Histogram = l.Histogram(c => c.Title)})
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Histogram[MoviesProvider.Movie1.Title].Should().BeOneOf(0, 1);
  }

  [TestMethod]
  public async Task Histogram_QueryEndPoint()
  {
    await TestHistogram(Context.CreateQuery<Movie>(MoviesProvider.MoviesTableName));
  }
    
  [TestMethod]
  public async Task CollectListStructs()
  {
    await CollectListStructs(Context.CreateQueryStream<Movie>(MoviesProvider.MoviesTableName));
  }

  //Struct(Name :='Karen', Age := 55)
  private class Person
  {
    public string Name { get; set; }
    public int Age { get; set; }
  }

  private async Task CollectListStructs(IQbservable<Movie> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new { Id = l.Key, Structs = l.CollectList(c => new Person { Age = 55, Name = "Karen" }) })
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Structs[0].Name.Should().Be("Karen");
  }

  [TestMethod]
  public async Task CollectListMaps()
  {
    await CollectListMaps(Context.CreateQueryStream<Movie>(MoviesProvider.MoviesTableName));
  }

  private async Task CollectListMaps(IQbservable<Movie> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    var dict = new Dictionary<string, int>()
    {
      ["Karen"] = 42
    };

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new { Id = l.Key, Maps = l.CollectList(c => dict) })
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Maps[0]["Karen"].Should().Be(42);
  }

  [TestMethod]
  public async Task CollectListArray()
  {
    await CollectListArray(Context.CreateQueryStream<Movie>(MoviesProvider.MoviesTableName));
  }

  private async Task CollectListArray(IQbservable<Movie> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new { Id = l.Key, Array = l.CollectList(c => new[] { 1, 2 }) })
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Array[0][1].Should().Be(2);
  }

  [TestMethod]
  public async Task CollectSetMaps()
  {
    await CollectSetMaps(Context.CreateQueryStream<Tweet>(TweetsStreamName));
  }

  private async Task CollectSetMaps(IQbservable<Tweet> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    var dict = new Dictionary<string, int>()
    {
      ["Karen"] = 42
    };

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new { Id = l.Key, Maps = l.CollectSet(c => dict) })
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Maps[0]["Karen"].Should().Be(42);
  }

  protected static string TweetsStreamName = "tweetsTest";

  [TestMethod]
  public async Task EarliestByOffsetMaps()
  {
    await EarliestByOffsetMaps(Context.CreateQueryStream<Tweet>(TweetsStreamName));
  }

  private async Task EarliestByOffsetMaps(IQbservable<Tweet> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    var dict = new Dictionary<string, int>()
    {
      ["Karen"] = 42,
      ["tums"] = 7
    };

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new { Id = l.Key, Maps = l.EarliestByOffset(c => dict) })
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == TweetsProvider.Tweet1.Id);
    id1.Maps["Karen"].Should().Be(42);
  }

  [TestMethod]
  public async Task LatestByOffsetMaps()
  {
    await LatestByOffsetMaps(Context.CreateQueryStream<Tweet>(TweetsStreamName));
  }

  private async Task LatestByOffsetMaps(IQbservable<Tweet> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    var dict = new Dictionary<string, int>()
    {
      ["Karen"] = 42
    };

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new { Id = l.Key, Maps = l.LatestByOffset(c => dict) })
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == TweetsProvider.Tweet1.Id);
    id1.Maps["Karen"].Should().Be(42);
  }

  [TestMethod]
  public async Task LatestByOffsetStructs()
  {
    await LatestByOffsetStructs(Context.CreateQueryStream<Tweet>(TweetsStreamName));
  }

  private async Task LatestByOffsetStructs(IQbservable<Tweet> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    var person = new Person {Age = 55, Name = "Karen"};

    var source = querySource
      .GroupBy(c => c.Id)
      .Select(l => new { Id = l.Key, Struct = l.LatestByOffset(c => person) })
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var id1 = actualValues.FirstOrDefault(c => c.Id == TweetsProvider.Tweet1.Id);
    id1.Struct.Name.Should().Be("Karen");
  }
}