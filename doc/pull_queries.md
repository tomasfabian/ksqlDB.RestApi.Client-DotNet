# Pull queries

**Pull queries** allow you to retrieve specific records from a stream or table based on certain criteria.
Unlike continuous queries, which continuously process and emit results in real-time, pull queries are used for ad-hoc retrieval of data from the stored state of a stream or table.

### Pull queries - `ExecutePullQuery`
**v1.0.0**

Execute [pull query](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/select-pull-query/) with plain string query:
```C#
string ksql = "SELECT * FROM avg_sensor_values WHERE SensorId = 'sensor-1';";
var result = await context.ExecutePullQuery<IoTSensorStats>(ksql);
```

### `CreatePullQuery<TEntity>`

[A pull query](https://docs.ksqldb.io/en/latest/concepts/queries/#pull) is a form of query issued by a client that retrieves a result as of "now", like a query against a traditional RDBS.

See also [GetManyAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet#ipullable---getmanyasync-v170).

```C#
using System.Net.Http;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Windows;

IKSqlDbRestApiClient restApiClient;

async Task Main()
{
  string ksqlDbUrl = @"http://localhost:8088";
  await using var context = new KSqlDBContext(ksqlDbUrl);

  var httpClient = new HttpClient
  {
    BaseAddress = new Uri(ksqlDbUrl)
  };

  var httpClientFactory = new HttpClientFactory(httpClient);
  restApiClient = new KSqlDbRestApiClient(httpClientFactory);
	
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
    .FirstOrDefaultAsync();

  Console.WriteLine($"{result?.SensorId} - {result?.AvgValue}");
}
```

The `CreateOrReplaceStreamAsync` method creates or replaces a stream with a specific schema and configuration by executing the provided SQL-like statement, and it returns the response from the execution as a `Task<HttpResponseMessage>`.
```C#
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
```

Inside the following method, we will generate an SQL INSERT statement using the sensor object's properties. The `sensor.SensorId` and `sensor.Value` values are interpolated into the statement string.

Then, the method calls another asynchronous method `ExecuteAsync` and passes the generated insert statement as an argument. It awaits the execution of the `ExecuteAsync` method and returns the resulting `Task<HttpResponseMessage>`.
```C#
async Task<HttpResponseMessage> InsertAsync(IoTSensor sensor)
{
  string insert =
    $"INSERT INTO sensor_values (SensorId, Value) VALUES ('{sensor.SensorId}', {sensor.Value});";

  return await ExecuteAsync(insert);
}
```

The bellow provided C# code defines an asynchronous method called `ExecuteAsync` that takes a string parameter `statement` and returns a `Task<HttpResponseMessage>`.

Inside the method, it creates a `KSqlDbStatement` object using the provided statement. It then calls an asynchronous method `ExecuteStatementAsync` on the `restApiClient` object, passing the `ksqlDbStatement` as a parameter.
The method awaits the execution of the statement and stores the resulting `HttpResponseMessage` in the httpResponseMessage variable.

Next, it reads the response content as a string using the `ReadAsStringAsync` method on `httpResponseMessage.Content` and assigns it to the `responseContent` variable.

Finally, the method returns the `httpResponseMessage`. The use of `ConfigureAwait(false)` ensures that the method does not capture the context during the continuation, which can improve performance in certain scenarios.

```C#
async Task<HttpResponseMessage> ExecuteAsync(string statement)
{
  KSqlDbStatement ksqlDbStatement = new(statement);

  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement)
        .ConfigureAwait(false);

  string responseContent = await httpResponseMessage.Content.ReadAsStringAsync()
        .ConfigureAwait(false);

  return httpResponseMessage;
}
```

```C#
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

### GetManyAsync
**v1.7.0**

- `IPullable.GetManyAsync<TEntity>` - Pulls all values from the materialized view asynchronously and terminates. 

```C#
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;

public static async Task<List<OrderData>> GetOrdersAsync()
{
  var ksqlDbUrl = @"http://localhost:8088";
  var options = new KSqlDBContextOptions(ksqlDbUrl) { ShouldPluralizeFromItemName = false };
  options.QueryParameters.Properties["ksql.query.pull.table.scan.enabled"] = "true";

  await using var context = new KSqlDBContext(options);
  var tableName = "queryable_order";
  var orderTypes = new List<int> { 1,3 };

  var enumerable = context.CreatePullQuery<OrderData>(tableName)    
    .Where(o => o.EventTime >= 1630886400 && o.EventTime <= 1630887401 && orderTypes.Contains(o.OrderType))
    .GetManyAsync();

  List<OrderData> list = new List<OrderData>();

  await foreach (var item in enumerable.ConfigureAwait(false))
  {
    Console.WriteLine(item.ToString());
    list.Add(item);
  } 

  return list;
}
```
```C#
public class OrderData: Record
{
  public int Id { get; set; }
  public long EventTime  { get; set; }
  public int OrderType { get; set; }
  public string Description { get; set; }
}
```

### `IPullable<T>.FirstOrDefaultAsync` (v1.0.0)

The `FirstOrDefaultAsync` method is specifically designed for asynchronous operations. It is an extension method available on collections or sequences that allows you to retrieve the first element, or the default value if no such element is found, in an asynchronous manner.

```C#
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;

private static async Task GetAsync(IPullable<IoTSensorStats> pullQuery)
{
  var result = await pullQuery
    .FirstOrDefaultAsync();

  Console.WriteLine(
    $"Pull query GetAsync result => Id: {result?.SensorId} - Avg Value: {result?.AvgValue} - Window Start {result?.WindowStart}");
}
```

### Window Bounds

The WHERE clause must contain a value for each primary-key column to retrieve and may optionally include bounds on WINDOWSTART and WINDOWEND if the materialized table is windowed.
```C#
using ksqlDB.RestApi.Client.KSql.Query.Functions;

const string MaterializedViewName = "avg_sensor_values";

string windowStart = "2019-10-03T21:31:16";
string windowEnd = "2025-10-03T21:31:16";

var result = await context.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
  .Where(c => c.SensorId == "sensor-1")
  .Where(c => Bounds.WindowStart > windowStart && Bounds.WindowEnd <= windowEnd)
  .GetAsync();
```

Generated KSQL:
```SQL
SELECT *
  FROM avg_sensor_values
 WHERE SensorId = 'sensor-1' AND (WINDOWSTART > '2019-10-03T21:31:16') AND (WINDOWEND <= '2020-10-03T21:31:16');
```
