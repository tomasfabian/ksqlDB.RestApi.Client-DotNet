# Pull queries

### `CreatePullQuery<TEntity>` (v0.10.0)

[A pull query](https://docs.ksqldb.io/en/latest/concepts/queries/#pull) is a form of query issued by a client that retrieves a result as of "now", like a query against a traditional RDBS.

See also [GetManyAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet#ipullable---getmanyasync-v170).


> ⚠ `IPullable<T>.GetAsync` was renamed to `IPullable<T>.FirstOrDefaultAsync` in version 2.0.0.

```C#
using System.Net.Http;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Windows;

IKSqlDbRestApiClient restApiClient;

async Task Main()
{
  string url = @"http:\\localhost:8088";
  await using var context = new KSqlDBContext(url);

  var http = new HttpClientFactory(new Uri(url));
  restApiClient = new KSqlDbRestApiClient(http);
	
  await CreateOrReplaceStreamAsync();
	
  var statement = context.CreateTableStatement("avg_sensor_values")
    .As<IoTSensor>("sensor_values")
    .GroupBy(c => c.SensorId)
    .WindowedBy(new TimeWindows(Duration.OfSeconds(5)).WithGracePeriod(Duration.OfHours(2)))
    .Select(c => new { SensorId = c.Key, AvgValue = c.Avg(g => g.Value) });

  var response = await statement.ExecuteStatementAsync();

  response = await InsertAsync(new IoTSensor { SensorId = "sensor-1", Value = 11 });
	
  var result = await context.CreatePullQuery<IoTSensorStats>("avg_sensor_values")
    .Where(c => c.SensorId == "sensor-1")
    .GetAsync();

  Console.WriteLine($"{result?.SensorId} - {result?.AvgValue}");
}

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

public record IoTSensor
{
  public string SensorId { get; init; }
  public int Value { get; init; }
}

public record IoTSensorStats
{
  public string SensorId { get; init; }
  public double AvgValue { get; init; }
}
```

We can use **backticks** to control the casing of the table's name.
```C#
context.CreatePullQuery<IoTSensorStats>("`IoT_sensor_stats`");
```

### Pull query Take extension method (Limit)
**v1.6.0**

Returns a specified number of contiguous elements from the start of a stream or a table. (ksqldb v0.24.0)
```C#
context.CreatePullQuery<Tweet>()
  .Take(2);
```
```SQL
SELECT * from tweets LIMIT 2;
```
