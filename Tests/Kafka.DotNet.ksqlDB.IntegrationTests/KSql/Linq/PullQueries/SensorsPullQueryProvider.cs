using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.IntegrationTests.Models.Sensors;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Windows;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq.PullQueries
{
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

      var statement = context.CreateOrReplaceTableStatement(MaterializedViewName)
        .As<IoTSensor>(StreamName)
        .GroupBy(c => c.SensorId)
        .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
        .Select(c => new { SensorId = c.Key, AvgValue = c.Avg(g => g.Value) });

      var response = await statement.ExecuteStatementAsync();

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
}