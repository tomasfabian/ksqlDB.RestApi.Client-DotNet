using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.Sample.Models.Sensors;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Kafka.DotNet.ksqlDB.KSql.Query.Windows;

namespace Kafka.DotNet.ksqlDB.Sample.PullQuery
{
  public class PullQueryExample
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
        .As<IoTSensor>("sensor_values")
        .GroupBy(c => c.SensorId)
        .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
        .Select(c => new { SensorId = c.Key, AvgValue = c.Avg(g => g.Value) });

      var query = statement.ToStatementString();

      var response = await statement.ExecuteStatementAsync();

      response = await InsertAsync(new IoTSensor { SensorId = "sensor-1", Value = 11 });
      
      await PullSensor(context);
    }

    private static async Task PullSensor(KSqlDBContext context)
    {
      string windowStart = "2019-10-03T21:31:16";
      string windowEnd = "2025-10-03T21:31:16";

      var result = await context.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
        .Where(c => c.SensorId == "sensor-1")
        .Where(c => Bounds.WindowStart > windowStart && Bounds.WindowEnd <= windowEnd)
        .GetAsync();

      Console.WriteLine($"Id: {result?.SensorId} - Avg Value: {result?.AvgValue} - Window Start {result?.WindowStart}");

      string ksql = "SELECT * FROM avg_sensor_values WHERE SensorId = 'sensor-1';";

      result = await context.ExecutePullQuery<IoTSensorStats>(ksql);
    }

    const string MaterializedViewName = "avg_sensor_values";

    async Task<HttpResponseMessage> CreateOrReplaceStreamAsync()
    {
      const string createOrReplaceStream =
    @"CREATE STREAM sensor_values (
    SensorId VARCHAR KEY,
    Value INT
) WITH (
    kafka_topic = 'sensor_values',
    partitions = 2,
    value_format = 'json'
);";

      return await ExecuteAsync(createOrReplaceStream);
    }

    async Task<HttpResponseMessage> InsertAsync(IoTSensor sensor)
    {
      string insert =
        $"INSERT INTO sensor_values (SensorId, Value) VALUES ('{sensor.SensorId}', {sensor.Value});";

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