using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.DotNetFramework.Sample.Models.Sensors;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.DotNetFramework.Sample.PullQuery;

public class PullQueryExample
{
  IKSqlDbRestApiClient? restApiClient;

  public async Task ExecuteAsync()
  {
    string ksqlDbUrl = @"http://localhost:8088";

    var contextOptions = new KSqlDbContextOptionsBuilder()
      .UseKSqlDb(ksqlDbUrl)
      .SetBasicAuthCredentials("fred", "letmein")
      .Options;

    contextOptions.DisposeHttpClient = false;

    await using var context = new KSqlDBContext(contextOptions);

    var httpClient = new HttpClient()
    {
      BaseAddress = new Uri(ksqlDbUrl)
    };

    var httpClientFactory = new HttpClientFactory(httpClient);

    restApiClient = new KSqlDbRestApiClient(httpClientFactory)
      .SetCredentials(new BasicAuthCredentials("fred", "letmein"));

    ((KSqlDbRestApiClient)restApiClient).DisposeHttpClient = false;

    await CreateOrReplaceStreamAsync();

    var statement = context.CreateTableStatement(MaterializedViewName)
      .As<IoTSensor>("sensor_values")
      .GroupBy(c => c.SensorId)
      .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
      .Select(c => new { SensorId = c.Key, AvgValue = c.Avg(g => g.Value) });

    var query = statement.ToStatementString();

    var response = await statement.ExecuteStatementAsync();
    var c = await response.Content.ReadAsStringAsync();

    response = await InsertAsync(new IoTSensor { SensorId = "sensor-1", Value = 11 });

    await PullSensor(context);
  }

  private static async Task PullSensor(KSqlDBContext context)
  {
    string windowStart = "2019-10-03T21:31:16";
    string windowEnd = "2025-10-03T21:31:16";

    var pullQuery = context.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
      .Where(c => c.SensorId == "sensor-1")
      .Where(c => Bounds.WindowStart > windowStart && Bounds.WindowEnd <= windowEnd)
      .Take(5);

    var sql = pullQuery.ToQueryString();

    await foreach (var item in pullQuery.GetManyAsync().OrderBy(c => c.SensorId).ConfigureAwait(false))
      Console.WriteLine($"Pull query - GetMany result => Id: {item?.SensorId} - Avg Value: {item?.AvgValue} - Window Start {item?.WindowStart}");

    var list = await pullQuery.GetManyAsync().OrderBy(c => c.SensorId).ToListAsync();
    string ksql = "SELECT * FROM avg_sensor_values WHERE SensorId = 'sensor-1';";

    var result2 = await context.ExecutePullQuery<IoTSensorStats>(ksql);
  }

  private static async Task GetAsync(IPullable<IoTSensorStats> pullQuery)
  {
    var result = await pullQuery
      .FirstOrDefaultAsync();

    Console.WriteLine(
      $"Pull query GetAsync result => Id: {result?.SensorId} - Avg Value: {result?.AvgValue} - Window Start {result?.WindowStart}");

    Console.WriteLine();
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

    var httpResponseMessage = await restApiClient!.ExecuteStatementAsync(ksqlDbStatement)
      .ConfigureAwait(false);

    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

    return httpResponseMessage;
  }
}
