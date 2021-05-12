using System;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.IntegrationTests.Models.Movies;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq
{
  public class MoviesProvider
  {
    private readonly KSqlDbRestApiProvider restApiProvider;

    public MoviesProvider(KSqlDbRestApiProvider restApiProvider)
    {
      this.restApiProvider = restApiProvider ?? throw new ArgumentNullException(nameof(restApiProvider));
    }

    public static readonly string MoviesTableName = "movies_test";
    public static readonly string ActorsTableName = "lead_actor_test";

    public async Task<bool> CreateTablesAsync()
    {
      var createMoviesTable = $@"CREATE OR REPLACE TABLE {MoviesTableName} (
        title VARCHAR PRIMARY KEY,
        id INT,
        release_year INT
      ) WITH (
        KAFKA_TOPIC='{MoviesTableName}',
        PARTITIONS=1,
        VALUE_FORMAT = 'JSON'
      );";
      
      KSqlDbStatement ksqlDbStatement = new(createMoviesTable);

      var result = await restApiProvider.ExecuteStatementAsync(ksqlDbStatement);
      var isSuccess = result.IsSuccess();
      
      isSuccess.Should().BeTrue();

      var createActorsTable = $@"CREATE OR REPLACE TABLE {ActorsTableName} (
        title VARCHAR PRIMARY KEY,
        actor_name VARCHAR
      ) WITH (
        KAFKA_TOPIC='{ActorsTableName}',
        PARTITIONS=1,
        VALUE_FORMAT='JSON'
      );";

      ksqlDbStatement = new KSqlDbStatement(createActorsTable);

      result = await restApiProvider.ExecuteStatementAsync(ksqlDbStatement);
      isSuccess = result.IsSuccess();
      
      isSuccess.Should().BeTrue();

      return true;
    }

    public static readonly Movie Movie1 = new()
    {
      Id = 1,
      Release_Year = 1986,
      Title = "Aliens"
    };

    public static readonly Movie Movie2 = new()
    {
      Id = 2,
      Release_Year = 1998,
      Title = "Die Hard"
    };

    public static readonly Lead_Actor LeadActor1 = new()
    {
      Actor_Name = "Sigourney Weaver",
      Title = "Aliens"
    };

    public static readonly Lead_Actor LeadActor2 = new()
    {
      Actor_Name = "Al Pacino",
      Title = "The Godfather"
    };

    public async Task<bool> InsertMovieAsync(Movie movie)
    {
      string insert =
        $"INSERT INTO {MoviesTableName} ({nameof(Movie.Id)}, {nameof(Movie.Title)}, {nameof(Movie.Release_Year)}) VALUES ({movie.Id}, '{movie.Title}', {movie.Release_Year});";
      
      KSqlDbStatement ksqlDbStatement = new(insert);

      var result = (await restApiProvider.ExecuteStatementAsync(ksqlDbStatement)).IsSuccess();      
      result.Should().BeTrue();

      return result;
    }

    public async Task<bool> InsertLeadAsync(Lead_Actor actor)
    {
      string insert =
        $"INSERT INTO {ActorsTableName} ({nameof(Lead_Actor.Title)}, {nameof(Lead_Actor.Actor_Name)}) VALUES ('{actor.Title}', '{actor.Actor_Name}');";
      
      KSqlDbStatement ksqlDbStatement = new(insert);

      var result = (await restApiProvider.ExecuteStatementAsync(ksqlDbStatement)).IsSuccess();      
      result.Should().BeTrue();

      return result;
    }

    public async Task DropTablesAsync()
    {
      await restApiProvider.DropTableAndTopic(ActorsTableName);
      await restApiProvider.DropTableAndTopic(MoviesTableName);
    }
  }
}