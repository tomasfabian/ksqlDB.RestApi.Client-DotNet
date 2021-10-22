using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq
{
  [TestClass]
  public class AggregationTests : Infrastructure.IntegrationTests
  {
    private static MoviesProvider moviesProvider;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      RestApiProvider = KSqlDbRestApiProvider.Create();

      moviesProvider = new MoviesProvider(RestApiProvider);

      await moviesProvider.CreateTablesAsync();

      await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
      await moviesProvider.InsertMovieAsync(MoviesProvider.Movie2);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
      await moviesProvider.DropTablesAsync();

      moviesProvider = null;
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
  }
}