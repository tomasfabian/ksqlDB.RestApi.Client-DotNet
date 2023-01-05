using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.Linq;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Query;

[TestClass]
public class KSqlNestedTypesTests : Infrastructure.IntegrationTests
{
  protected static MoviesProvider MoviesProvider = null!;

  [ClassInitialize]
  public static async Task ClassInitialize(TestContext context)
  {
    await InitializeDatabase();
  }

  protected static async Task InitializeDatabase()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();

    MoviesProvider = new MoviesProvider(RestApiProvider);
    await MoviesProvider.CreateTablesAsync();

    await MoviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
  }

  [ClassCleanup]
  public static async Task ClassCleanup()
  {
    await MoviesProvider.DropTablesAsync();
  }

  protected string MoviesTableName => MoviesProvider.MoviesTableName;

  protected virtual IQbservable<Movie> MoviesStream => Context.CreateQueryStream<Movie>(MoviesTableName);

  [TestMethod]
  public async Task ArrayInArray()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expected = new[]
    {
      new[] {1, 2},
      new[] {3, 4},
    };

    //Act
    var source = MoviesStream.Select(c => new
    {
      Arr = new[]
      {
        new[] {1, 2},
        new[] {3, 4},
      }, c.Title
    }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actualArr = actualValues.First().Arr;

    actualArr[0][0].Should().Be(expected[0][0]);
    actualArr[0][1].Should().Be(expected[0][1]);
    actualArr[1][0].Should().Be(expected[1][0]);
    actualArr[1][1].Should().Be(expected[1][1]);
  }

  [TestMethod]
  public async Task ArrayInMap()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expected = new Dictionary<string, int[]>
    {
      { "a", new[] { 1, 2 } },
      { "b", new[] { 3, 4 } },
    };

    //Act
    var source = MoviesStream
      .Select(c => new
      {
        Map = new Dictionary<string, int[]>
        {
          { "a", new[] { 1, 2 } },
          { "b", new[] { 3, 4 } },
        }, c.Title
      }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actual = actualValues.First().Map;

    actualValues.First().Title.Should().Be(MoviesProvider.Movie1.Title);

    actual["a"][0].Should().Be(expected["a"][0]);
    actual["a"][1].Should().Be(expected["a"][1]);
    actual["b"][0].Should().Be(expected["b"][0]);
    actual["b"][1].Should().Be(expected["b"][1]);
  }

  [TestMethod]
  public async Task MapInArray()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expected = new[]
    {
      new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
      new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
    };

    //Act
    var source = MoviesStream
      .Select(c => new
      {
        Arr = new[]
        {
          new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
          new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
        }, c.Release_Year
      }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actual = actualValues.First().Arr;

    actualValues.First().Release_Year.Should().Be(MoviesProvider.Movie1.Release_Year);

    actual[0]["a"].Should().Be(expected[0]["a"]);
    actual[1]["c"].Should().Be(expected[1]["c"]);
    actual[0]["b"].Should().Be(expected[0]["b"]);
    actual[1]["d"].Should().Be(expected[1]["d"]);
  }

  [TestMethod]
  public async Task MapInMap()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expected = new Dictionary<string, Dictionary<string, int>>
    {
      { "x", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
      { "y", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
    };

    //Act
    var source = MoviesStream
      .Select(c => new
      {
        Map = new Dictionary<string, Dictionary<string, int>>
        {
          { "x", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
          { "y", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
        }, c.Id
      }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actual = actualValues.First().Map;

    actual["x"]["a"].Should().Be(expected["x"]["a"]);
    actual["y"]["c"].Should().Be(expected["y"]["c"]);
    actual["x"]["b"].Should().Be(expected["x"]["b"]);
    actual["y"]["d"].Should().Be(expected["y"]["d"]);
  }

  private struct MovieStruct
  {
    public string Title { get; set; }

    public int Id { get; set; }
  }

  [TestMethod]
  public async Task Struct()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expected = new MovieStruct { Title = "ET", Id = 2};

    //Act
    var source = MoviesStream.Select(c => new
    {
      Str = new MovieStruct { Title = "ET", Id = 2}, c.Release_Year
    }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actualStr = actualValues.First().Str;

    actualStr.Title.Should().Be(expected.Title);
    actualStr.Id.Should().Be(expected.Id);
    actualValues.First().Release_Year.Should().Be(MoviesProvider.Movie1.Release_Year);
  }

  [TestMethod]
  public async Task Struct_FromColumns()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expectedMovie = MoviesProvider.Movie1;

    //Act
    var source = MoviesStream.Select(c => new
    {
      Str = new MovieStruct { Title = c.Title, Id = c.Id}, c.Release_Year
    }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actualStr = actualValues.First().Str;

    actualStr.Title.Should().Be(expectedMovie.Title);
    actualStr.Id.Should().Be(expectedMovie.Id);
    actualValues.First().Release_Year.Should().Be(expectedMovie.Release_Year);
  }

  [TestMethod]
  public async Task ArrayWithNestedStruct()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expectedMovie = MoviesProvider.Movie1;

    //Act
    var source = MoviesStream
      .Select(c => new
      {
        Arr = new[]
        {
          new MovieStruct
          {
            Title = c.Title,
            Id = c.Id,
          }, 
          new MovieStruct
          {
            Title = "test",
            Id = 1,
          }
        }, c.Release_Year,
      }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actualStr = actualValues.First().Arr;

    actualStr[0].Title.Should().Be(expectedMovie.Title);
    actualStr[0].Id.Should().Be(expectedMovie.Id);
    actualValues.First().Release_Year.Should().Be(expectedMovie.Release_Year);
  }

  [TestMethod]
  public async Task Array_FromColumn()
  {
    //Arrange
    int expectedItemsCount = 1;
    var expectedMovie = MoviesProvider.Movie1;

    //Act
    var source = MoviesStream.Select(c => new
    {
      Arr = new[]
      {
        c.Id, c.Release_Year
      }, c.Title
    }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    var actualArr = actualValues.First().Arr;

    actualArr[0].Should().Be(expectedMovie.Id);
    actualArr[1].Should().Be(expectedMovie.Release_Year);
    actualValues.First().Title.Should().Be(expectedMovie.Title);
  }
}
