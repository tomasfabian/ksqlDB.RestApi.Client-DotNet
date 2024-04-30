using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDb.RestApi.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Query;

public class KSqlLexicalPrecedenceTests : Infrastructure.IntegrationTests
{
  private static MoviesProvider moviesProvider = null!;

  [OneTimeSetUp]
  public static async Task ClassInitialize()
  {
    await InitializeDatabase();
  }

  protected static async Task InitializeDatabase()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();

    moviesProvider = new MoviesProvider(RestApiProvider);
    await moviesProvider.CreateTablesAsync();

    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
    await moviesProvider.InsertMovieAsync(MoviesProvider.Movie2);
  }

  [OneTimeTearDown]
  public static async Task ClassCleanup()
  {
    await moviesProvider.DropTablesAsync();
  }

  protected static string MoviesTableName => MoviesProvider.MoviesTableName;

  protected virtual IQbservable<Movie> MoviesStream => Context.CreatePushQuery<Movie>(MoviesTableName);

  [Test]
  public async Task Select()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = MoviesStream.Select(c => new
    {
      First = c.Id + 2 * 3,
      ChangedOrder = (c.Id + 2) * 3
    }).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var actualArr = actualValues.First();

    actualArr.First.Should().Be(7);
    actualArr.ChangedOrder.Should().Be(9);
  }

  [Test]
  public async Task Where()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = MoviesStream.Where(c => (c.Title == "Aliens" || c.Title == "Die Hard") && c.Release_Year < 1988).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var movie = actualValues.First();

    movie.Id.Should().Be(MoviesProvider.Movie1.Id);
  }

  [Test]
  public async Task Where_NoBrackets()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = MoviesStream.Where(c => c.Title == "Aliens" || c.Title == "Die Hard" && c.Release_Year < 1988).ToAsyncEnumerable();

    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    var movie = actualValues.First();

    movie.Id.Should().Be(MoviesProvider.Movie1.Id);
  }
}
