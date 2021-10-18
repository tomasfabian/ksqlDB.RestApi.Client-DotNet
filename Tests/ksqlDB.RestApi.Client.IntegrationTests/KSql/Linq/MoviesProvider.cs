using System;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq
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
      var insertProperties = new InsertProperties()
      {
        EntityName = MoviesTableName,
        ShouldPluralizeEntityName = false
      };

      var result = (await restApiProvider.InsertIntoAsync(movie, insertProperties)).IsSuccess(); 

      result.Should().BeTrue();

      return result;
    }

    public async Task<bool> InsertLeadAsync(Lead_Actor actor)
    {
      var insertProperties = new InsertProperties()
      {
        EntityName = ActorsTableName,
        ShouldPluralizeEntityName = false
      };
      
      var result = (await restApiProvider.InsertIntoAsync(actor, insertProperties)).IsSuccess(); 

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