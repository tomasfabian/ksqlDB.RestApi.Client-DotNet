using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDB.RestApi.Client.Samples.Models.Movies;

namespace ksqlDB.RestApi.Client.Samples.Providers;

public class MoviesProvider(IKSqlDbRestApiProvider restApiProvider)
{
  public static readonly string MoviesTableName = "movies";
  public static readonly string ActorsTableName = "lead_actor";

  public static readonly Movie Movie1 = new()
  {
    Id = 1,
    Release_Year = 1986,
    Title = "Aliens"
  };

  public static readonly Movie Movie2 = new()
  {
    Id = 2,
    Release_Year = 1988,
    Title = "Die Hard"
  };

  public static readonly Lead_Actor LeadActor1 = new()
  {
    Actor_Name = "Sigourney Weaver",
    Title = "Aliens"
  };

  private readonly IKSqlDbRestApiProvider restApiProvider = restApiProvider ?? throw new ArgumentNullException(nameof(restApiProvider));

  public async Task<bool> CreateTablesAsync(CancellationToken cancellationToken = default)
  {
    EntityCreationMetadata metadata = new(MoviesTableName, 1)
    {
      Replicas = 1,
      ValueFormat = SerializationFormats.Json
    };

    var result = await restApiProvider.CreateOrReplaceTableAsync<Movie>(metadata, cancellationToken);
    if (!result.IsSuccessStatusCode)
    {
      var content = await result.Content.ReadAsStringAsync(cancellationToken);
      Console.WriteLine(content);
    }

    var createActorsTable = $@"CREATE OR REPLACE TABLE {ActorsTableName} (
        title VARCHAR PRIMARY KEY,
        actor_name VARCHAR
      ) WITH (
        KAFKA_TOPIC='{ActorsTableName}',
        PARTITIONS=1,
        VALUE_FORMAT='JSON'
      );";

    var ksqlDbStatement = new KSqlDbStatement(createActorsTable);
    await restApiProvider.ExecuteStatementAsync(ksqlDbStatement, cancellationToken);

    return true;
  }

  public async Task<HttpResponseMessage> InsertMovieAsync(Movie movie)
  {
    var insertStatement = restApiProvider.ToInsertStatement(movie);
    Console.WriteLine(insertStatement.Sql);

    var result = await restApiProvider.InsertIntoAsync(movie, new InsertProperties {ShouldPluralizeEntityName = true});

    var content = await result.Content.ReadAsStringAsync();
    var responses = await result.ToStatementResponsesAsync();

    return result;
  }

  public async Task<HttpResponseMessage> InsertLeadAsync(Lead_Actor actor)
  {
    var insert =
      $"INSERT INTO {ActorsTableName} ({nameof(Lead_Actor.Title)}, {nameof(Lead_Actor.Actor_Name)}) VALUES ('{actor.Title}', '{actor.Actor_Name}');";

    KSqlDbStatement ksqlDbStatement = new(insert);

    var result = await restApiProvider.ExecuteStatementAsync(ksqlDbStatement);

    return result;
  }

  public async Task DropTablesAsync()
  {
    await restApiProvider.DropTableAndTopic(ActorsTableName);
    await restApiProvider.DropTableAndTopic(MoviesTableName);
  }
}
