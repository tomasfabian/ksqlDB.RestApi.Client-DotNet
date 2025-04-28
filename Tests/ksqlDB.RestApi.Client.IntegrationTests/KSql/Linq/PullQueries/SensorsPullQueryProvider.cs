using ksqlDb.RestApi.Client.IntegrationTests.Http;
using ksqlDb.RestApi.Client.IntegrationTests.Models.Sensors;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq.PullQueries;

internal class SensorsPullQueryProvider
{
  private static string Url => "http://localhost:8088";

  private readonly KSqlDbRestApiClient restApiClient;

  public SensorsPullQueryProvider()
  {
    var http = new HttpClientFactory(new Uri(Url));
    restApiClient = new KSqlDbRestApiClient(http);
  }

  public async Task<StatementResponse> CreateTableAsync()
  {
    await using var context = new KSqlDBContext(Url);

    await CreateOrReplaceStreamAsync();

    var windowDuration = Duration.OfMilliseconds(100);

    var statement = context.CreateTableStatement(MaterializedViewName)
      .As<IoTSensor>(StreamName)
      .GroupBy(c => c.SensorId)
      .WindowedBy(new TimeWindows(windowDuration).WithGracePeriod(Duration.OfHours(2)))
      .Select(c => new {SensorId = c.Key, AvgValue = c.Avg(g => g.Value)});

    var s = statement.ToStatementString();
    var response = await statement.ExecuteStatementAsync();
    var statementResponses = await response.ToStatementResponsesAsync();

    await Task.Delay(TimeSpan.FromSeconds(10));

    return statementResponses[0];
  }

  public Task<HttpResponseMessage> InsertSensorAsync(string sensorId)
  {
    return InsertAsync(new IoTSensor { SensorId = sensorId, Value = new Random().Next(1, 100) });
  }

  public async Task DropEntitiesAsync()
  {
    await restApiClient.DropTableAsync(MaterializedViewName, useIfExistsClause: true, deleteTopic: true);
    await restApiClient.DropStreamAsync(StreamName, useIfExistsClause: true, deleteTopic: true);
  }

  internal const string MaterializedViewName = "avg_sensor_values";

  internal static string StreamName => "test_sensor_values";

  async Task CreateOrReplaceStreamAsync()
  {
    string createOrReplaceStream =
      $@"CREATE OR REPLACE STREAM {StreamName} (
    SensorId VARCHAR KEY,
    Value INT
) WITH (
    kafka_topic = '{StreamName}',
    partitions = 2,
    value_format = 'json'
);";

    await ExecuteAsync(createOrReplaceStream);
  }

  async Task<HttpResponseMessage> InsertAsync(IoTSensor sensor)
  {
    string insert =
      $"INSERT INTO {StreamName} (SensorId, Value) VALUES ('{sensor.SensorId}', {sensor.Value});";

    return await ExecuteAsync(insert);
  }

  async Task<HttpResponseMessage> ExecuteAsync(string statement)
  {
    KSqlDbStatement ksqlDbStatement = new(statement);

    var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement)
      .ConfigureAwait(false);

    string _ = await httpResponseMessage.Content.ReadAsStringAsync();

    return httpResponseMessage;
  }
}
