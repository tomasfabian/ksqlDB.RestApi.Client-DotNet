using System;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq
{
  [TestClass]
  public class JoinsTests : Infrastructure.IntegrationTests
  {
    private static MoviesProvider moviesProvider;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      RestApiProvider = KSqlDbRestApiProvider.Create();
      
      moviesProvider = new MoviesProvider(RestApiProvider);
      
      await moviesProvider.DropTablesAsync();
      
      await Task.Delay(TimeSpan.FromSeconds(1));

      await moviesProvider.CreateTablesAsync();

      await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
      await moviesProvider.InsertLeadAsync(MoviesProvider.LeadActor1);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
      await moviesProvider.DropTablesAsync();

      moviesProvider = null;
    }

    private string MoviesTableName => MoviesProvider.MoviesTableName;
    private string ActorsTableName => MoviesProvider.ActorsTableName;

    [TestMethod]
    public async Task Join()
    {
      //Arrange
      int expectedItemsCount = 1;

      var source = Context.CreateQueryStream<Movie>(MoviesTableName)
        .Join(
          Source.Of<Lead_Actor>(ActorsTableName),
          movie => movie.Title,
          actor => actor.Title,
          (movie, actor) => new
          {
            movie.Id,
            Title = movie.Title,
            movie.Release_Year,
            ActorTitle = actor.Title,
            Substr = K.Functions.Substring(actor.Title, 2, 4)
          }
        )
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);

      Assert.AreEqual(MoviesProvider.Movie1.Title, actualValues[0].Title);
      Assert.AreEqual(MoviesProvider.LeadActor1.Title, actualValues[0].Title);
      Assert.AreEqual("lien", actualValues[0].Substr);
      Assert.AreEqual(MoviesProvider.Movie1.Release_Year, actualValues[0].Release_Year);
    }

    [TestMethod]
    public async Task LeftJoin()
    {
      //Arrange
      int expectedItemsCount = 2;

      var source = Context.CreateQueryStream<Movie>(MoviesTableName)
        .LeftJoin(
          Source.Of<Lead_Actor>(ActorsTableName),
          movie => movie.Title,
          actor => actor.Title,
          (movie, actor) => new
          {
            movie.Id,
            Title = movie.Title,
            movie.Release_Year,
            ActorTitle = actor.Title,
            Substr = K.Functions.Substring(actor.Title, 2, 4)
          }
        )
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);

      Assert.AreEqual(MoviesProvider.Movie1.Title, actualValues[0].Title);
      Assert.AreEqual(MoviesProvider.LeadActor1.Title, actualValues[0].Title);

      Assert.AreEqual("lien", actualValues[1].Substr);
      Assert.AreEqual(MoviesProvider.Movie1.Release_Year, actualValues[1].Release_Year);
      Assert.AreEqual(MoviesProvider.LeadActor1.Title, actualValues[1].ActorTitle);
    }

    public record Movie2 : Record
    {
      public string Title { get; set; }
      public int? Id { get; set; }
      public int? Release_Year { get; set; }
    }

    [TestMethod]
    public async Task FullOuterJoin()
    {
      await FullOuterJoinTest(Context.CreateQueryStream<Movie2>(MoviesTableName));
    }

    [TestMethod]
    public async Task FullOuterJoin_QueryEndPoint()
    {
      await FullOuterJoinTest(Context.CreateQuery<Movie2>(MoviesTableName));
    }

    public async Task FullOuterJoinTest(IQbservable<Movie2> querySource)
    {
      //Arrange
      int expectedItemsCount = 3;
        
      await moviesProvider.InsertLeadAsync(MoviesProvider.LeadActor2);

      var source = querySource
        .FullOuterJoin(
          Source.Of<Lead_Actor>(ActorsTableName),
          movie => movie.Title,
          actor => actor.Title,
          (movie, actor) => new
          {
            movie.Id,
            Title = movie.Title,
            movie.Release_Year,
            ActorTitle = actor.Title
          }
        )
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);
        
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);

      actualValues[2].Id.Should().BeNull();
      actualValues[2].Release_Year.Should().BeNull();
      actualValues[2].Title.Should().BeNull();
      Assert.AreEqual(MoviesProvider.LeadActor2.Title, actualValues[2].ActorTitle);
    }
  }
}