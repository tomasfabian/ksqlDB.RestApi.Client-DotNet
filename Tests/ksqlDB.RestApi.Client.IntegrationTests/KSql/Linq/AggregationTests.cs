using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDb.RestApi.Client.IntegrationTests.Models;
using ksqlDb.RestApi.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;

public class AggregationTests : Infrastructure.IntegrationTests
{
  private static MoviesProvider moviesProvider = null!;
  private static TweetsProvider? tweetsProvider;

  private static readonly string TweetsTopicName = "tweetsTestTopic";

  [OneTimeSetUp]
  public static async Task ClassInitialize()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();

    moviesProvider = new MoviesProvider(RestApiProvider);

    await moviesProvider.CreateTablesAsync();

    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie2);

    tweetsProvider = new TweetsProvider(RestApiProvider);

    await tweetsProvider.CreateTweetsStream(TweetsStreamName, TweetsTopicName);

    await tweetsProvider.InsertTweetAsync(TweetsProvider.Tweet1, TweetsStreamName);
    await tweetsProvider.InsertTweetAsync(TweetsProvider.Tweet2, TweetsStreamName);
  }

  [OneTimeTearDown]
  public static async Task ClassCleanup()
  {
    await moviesProvider.DropTablesAsync();

    moviesProvider = null!;
    tweetsProvider = null;
  }

  [Test]
  public async Task Histogram()
  {
    await TestHistogram(Context.CreatePushQuery<Movie>(MoviesProvider.MoviesTableName));
  }

  private static async Task TestHistogram(IQbservable<Movie> querySource)
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
    var id1 = actualValues.First(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Histogram[MoviesProvider.Movie1.Title].Should().BeOneOf(0, 1);
  }

  [Test]
  public async Task Histogram_QueryEndPoint()
  {
    Context = CreateKSqlDbContext(EndpointType.Query);
    await TestHistogram(Context.CreatePushQuery<Movie>(MoviesProvider.MoviesTableName));
  }
    
  [Test]
  public async Task CollectListStructs()
  {
    await CollectListStructs(Context.CreatePushQuery<Movie>(MoviesProvider.MoviesTableName));
  }

  //Struct(Name :='Karen', Age := 55)
  private class Person
  {
    public string Name { get; init; } = null!;
    public int Age { get; set; }
  }

  private static async Task CollectListStructs(IQbservable<Movie> querySource)
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
    var id1 = actualValues.First(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Structs[0].Name.Should().Be("Karen");
  }

  [Test]
  public async Task CollectListMaps()
  {
    await CollectListMaps(Context.CreatePushQuery<Movie>(MoviesProvider.MoviesTableName));
  }

  private static async Task CollectListMaps(IQbservable<Movie> querySource)
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
    var id1 = actualValues.First(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Maps[0]["Karen"].Should().Be(42);
  }

  [Test]
  public async Task CollectListArray()
  {
    await CollectListArray(Context.CreatePushQuery<Movie>(MoviesProvider.MoviesTableName));
  }

  private static async Task CollectListArray(IQbservable<Movie> querySource)
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
    var id1 = actualValues.First(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Array[0][1].Should().Be(2);
  }

  [Test]
  public async Task CollectSetMaps()
  {
    await CollectSetMaps(Context.CreatePushQuery<Tweet>(TweetsStreamName));
  }

  private static async Task CollectSetMaps(IQbservable<Tweet> querySource)
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
    var id1 = actualValues.First(c => c.Id == MoviesProvider.Movie1.Id);
    id1.Maps[0]["Karen"].Should().Be(42);
  }

  protected static string TweetsStreamName => "tweetsTest";

  [Test]
  public async Task EarliestByOffsetMaps()
  {
    await EarliestByOffsetMaps(Context.CreatePushQuery<Tweet>(TweetsStreamName));
  }

  private static async Task EarliestByOffsetMaps(IQbservable<Tweet> querySource)
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
    var id1 = actualValues.First(c => c.Id == TweetsProvider.Tweet1.Id);
    id1.Maps["Karen"].Should().Be(42);
  }

  [Test]
  public async Task LatestByOffsetMaps()
  {
    await LatestByOffsetMaps(Context.CreatePushQuery<Tweet>(TweetsStreamName));
  }

  private static async Task LatestByOffsetMaps(IQbservable<Tweet> querySource)
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
    var id1 = actualValues.First(c => c.Id == TweetsProvider.Tweet1.Id);
    id1.Maps["Karen"].Should().Be(42);
  }

  [Test]
  public async Task LatestByOffsetStructs()
  {
    await LatestByOffsetStructs(Context.CreatePushQuery<Tweet>(TweetsStreamName));
  }

  private static async Task LatestByOffsetStructs(IQbservable<Tweet> querySource)
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
    var id1 = actualValues.First(c => c.Id == TweetsProvider.Tweet1.Id);
    id1.Struct.Name.Should().Be("Karen");
  }
}
