using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
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
      var id1 = actualValues[0];
      id1.Id.Should().Be(MoviesProvider.Movie1.Id);
      id1.Histogram[MoviesProvider.Movie1.Title].Should().Be(1);

      var id2 = actualValues[1];
      id2.Id.Should().Be(MoviesProvider.Movie2.Id);
      id2.Histogram[MoviesProvider.Movie2.Title].Should().Be(1);
    }

    [TestMethod]
    public async Task Histogram_QueryEndPoint()
    {
      await TestHistogram(Context.CreateQuery<Movie>(MoviesProvider.MoviesTableName));
    }
  }
}