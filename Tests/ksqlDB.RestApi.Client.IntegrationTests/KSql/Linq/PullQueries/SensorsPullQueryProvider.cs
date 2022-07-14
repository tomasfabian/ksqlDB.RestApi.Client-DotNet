using System;
using System.Net.Http;
using System.Threading.Tasks;
using ksqlDB.Api.Client.IntegrationTests.Http;
using ksqlDB.Api.Client.IntegrationTests.Models.Sensors;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq.PullQueries;

internal class SensorsPullQueryProvider
{
  IKSqlDbRestApiClient restApiClient;

  public async Task ExecuteAsync()
  {
    string url = @"http:\\localhost:8088";
    await using var context = new KSqlDBContext(url);

    var http = new HttpClientFactory(new Uri(url));
    restApiClient = new KSqlDbRestApiClient(http);

    await CreateOrReplaceStreamAsync();

    var statement = context.CreateTableStatement(MaterializedViewName)
      .As<IoTSensor>(StreamName)
      .GroupBy(c => c.SensorId)
      .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
      .Select(c => new { SensorId = c.Key, AvgValue = c.Avg(g => g.Value) });

    var response = await statement.ExecuteStatementAsync();
    var statementResponses = await response.ToStatementResponsesAsync();

    await Task.Delay(TimeSpan.FromSeconds(1));

    response = await InsertAsync(new IoTSensor { SensorId = "sensor-1", Value = 11 });
  }

  internal const string MaterializedViewName = "avg_sensor_values";

  internal string StreamName => "test_sensor_values";

  async Task<HttpResponseMessage> CreateOrReplaceStreamAsync()
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

    return await ExecuteAsync(createOrReplaceStream);
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

    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

    return httpResponseMessage;
  }
}