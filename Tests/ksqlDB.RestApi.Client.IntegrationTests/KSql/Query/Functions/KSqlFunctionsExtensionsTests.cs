using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDb.RestApi.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Query.Functions;

public class KSqlFunctionsExtensionsTests : Infrastructure.IntegrationTests
{
  private static MoviesProvider moviesProvider = null!;

  [OneTimeSetUp]
  public static async Task ClassInitialize()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();

    moviesProvider = new MoviesProvider(RestApiProvider);
    await moviesProvider.CreateTablesAsync();

    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
  }

  [OneTimeTearDown]
  public static async Task ClassCleanup()
  {
    await moviesProvider.DropTablesAsync();
  }

  private string MoviesTableName => MoviesProvider.MoviesTableName;

  [Test]
  public async Task DateToString()
  {
    await DateToStringTest(Context.CreatePushQuery<Movie>(MoviesTableName));
  }

  [Test]
  public async Task DateToString_QueryEndPoint()
  {
    Context = CreateKSqlDbContext(EndpointType.Query);
    await DateToStringTest(Context.CreatePushQuery<Movie>(MoviesTableName));
  }

  public async Task DateToStringTest(IQbservable<Movie> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    int epochDays = 18672;
    string format = "yyyy-MM-dd";
    Expression<Func<Movie, string>> expression = _ => KSqlFunctions.Instance.DateToString(epochDays, format);
      
    //Act
    var source = querySource
      .Select(expression)
      //.Select(c => new { DTS = KSqlFunctions.Instance.DateToString(epochDays, format) })
      .ToAsyncEnumerable();
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Should().BeEquivalentTo("2021-02-14");
  }

  [Test]
  public async Task Entries()
  {
    await EntriesTest(Context.CreatePushQuery<Movie>(MoviesTableName));
  }

  [Test]
  public async Task Entries_QueryEndPoint()
  {
    Context = CreateKSqlDbContext(EndpointType.Query);
    await EntriesTest(Context.CreatePushQuery<Movie>(MoviesTableName));
  }

  public async Task EntriesTest(IQbservable<Movie> querySource)
  {
    //Arrange
    int expectedItemsCount = 1;

    bool sorted = true;

    //Act
    var source = querySource
      .Select(c => new { Col = KSqlFunctions.Instance.Entries(new Dictionary<string, string>()
      {
        { "a", "value" }
      }, sorted)})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Col[0].K.Should().BeEquivalentTo("a");
    actualValues[0].Col[0].V.Should().BeEquivalentTo("value");
  }

  [Test]
  public async Task ArrayIntersect()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = KSqlFunctions.Instance.ArrayIntersect(new [] { 1, 2 }, new []{ 1 } )})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Col.Length.Should().Be(1);
    actualValues[0].Col[0].Should().Be(1);
  }

  [Test]
  public async Task ArrayJoin()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = KSqlFunctions.Instance.ArrayJoin(new [] { 1, 2 }, ";" )})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues.Count.Should().Be(1);
    actualValues[0].Col.Should().Be("1;2");
  }

  [Test]
  public async Task ArrayLength()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = KSqlFunctions.Instance.ArrayLength(new [] { 1, 2 } )})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues.Count.Should().Be(1);
    actualValues[0].Col.Should().Be(2);
  }

  [Test]
  public async Task ArrayMin()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = KSqlFunctions.Instance.ArrayMin(new [] { 1, 2 } )})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues.Count.Should().Be(1);
    actualValues[0].Col.Should().Be(1);
  }

  [Test]
  [Ignore("Cannot construct an array with all NULL elements")]
  public async Task ArrayMin_Null()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = KSqlFunctions.Instance.ArrayMin(new string [] { })})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues.Count.Should().Be(1);
    actualValues[0].Col.Should().BeNull();
  }

  [Test]
  public async Task ArrayRemove()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = KSqlFunctions.Instance.ArrayRemove(new [] { 1, 2 }, 2)})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues.Count.Should().Be(1);
    actualValues[0].Col.Length.Should().Be(1);
  }

  [Test]
  public async Task ArraySort()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.ArraySort(new int?[]{ 3, null, 1}, ListSortDirection.Ascending)})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues[0].Col.Should().BeEquivalentTo(new int?[] { 1, 3, null });
  }

  [Test]
  public async Task ArrayUnion()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.ArrayUnion(new int?[]{ 3, null, 1}, new int?[]{ 4, null})})
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues[0].Col.Should().BeEquivalentTo(new int?[] { 3, null, 1, 4 });
  }

  [Test]
  public async Task Concat()
  {
    //Arrange
    int expectedItemsCount = 1;
    string message = "_Hello";

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.Concat(c.Title, message), ColWS = K.Functions.ConcatWS(" - ", c.Title, message) })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Col.Should().Be($"{MoviesProvider.Movie1.Title}{message}");
    actualValues[0].ColWS.Should().Be($"{MoviesProvider.Movie1.Title} - {message}");
  }

  [Test]
  public async Task ToBytes()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context
      .CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.ToBytes(c.Title, "utf8") })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);

    string result = Encoding.UTF8.GetString(actualValues[0].Col);
    result.Should().Be(MoviesProvider.Movie1.Title);
  }

  [Test]
  public async Task FromBytes()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.FromBytes(K.Functions.ToBytes(c.Title, "utf8"), "utf8") })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);

    actualValues[0].Col.Should().BeEquivalentTo(MoviesProvider.Movie1.Title);
  }

  [Test]
  [Ignore("TODO")]
  public async Task FromBytes_CapturedVariable()
  {
    //Arrange
    int expectedItemsCount = 1;
    byte[] bytes = Encoding.UTF8.GetBytes(MoviesProvider.Movie1.Title);
    //QWxpZW4=

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.FromBytes(bytes, "utf8") })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);

    actualValues[0].Col.Should().BeEquivalentTo(MoviesProvider.Movie1.Title);
  }

  [Test]
  public async Task AsMap()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.AsMap(new []{ "1", "2" }, new []{ 11, 22 }) })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Col["1"].Should().Be(11);
  }

  [Test]
  public async Task JsonArrayContains()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context
      .CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.JsonArrayContains("[1, 2, 3]", 2) })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Col.Should().BeTrue();
  }

  [Test]
  public async Task MapKeys()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Col = K.Functions.MapKeys(new Dictionary<string, int>
      {
        {"apple", 10},
        {"banana", 20}
      }) })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Col[0].Should().Be("banana");
    actualValues[0].Col[1].Should().Be("apple");
  }

  [Test]
  public async Task Encode()
  {
    //Arrange
    int expectedItemsCount = 1;

    string inputEncoding = "utf8";
    string outputEncoding = "ascii";
    Expression<Func<Movie, string>> expression = c => K.Functions.Encode(c.Title, inputEncoding, outputEncoding);

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(expression)
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Should().Be(MoviesProvider.Movie1.Title);
  }

  [Test]
  public async Task ExtractJsonField()
  {
    //Arrange
    int expectedItemsCount = 1;

    string json =
      "{\r\n   \"log\": {\r\n      \"cloud\": \"gcp836Csd\",\r\n      \"app\": \"ksProcessor\",\r\n      \"instance\": 4\r\n   }\r\n}";

    string jsonPath = "$.log.cloud";

    //Act
    var source = Context.CreatePushQuery<Movie>(MoviesTableName)
      .Select(c => new { Extracted = K.Functions.ExtractJsonField(json, jsonPath) })
      .ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);
    actualValues[0].Extracted.Should().Be("gcp836Csd");
  }
}
