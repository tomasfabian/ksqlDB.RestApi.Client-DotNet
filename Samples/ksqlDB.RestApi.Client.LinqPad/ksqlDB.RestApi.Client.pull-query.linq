<Query Kind="Program">
  <NuGetReference>ksqlDB.RestApi.Client</NuGetReference>
  <Namespace>ksqlDB.RestApi.Client.KSql.Linq.PullQueries</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Linq.Statements</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query.Context</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query.Functions</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query.Windows</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Statements</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Http</Namespace>
</Query>

IKSqlDbRestApiClient restApiClient;

async Task Main()
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

	query.Dump();

	var response = await statement.ExecuteStatementAsync();

	response = await InsertAsync(new IoTSensor { SensorId = "sensor-1", Value = 11 });

	//await Task.Delay(10000);
	string windowStart = "2019-10-03T21:31:16";
	string windowEnd = "2025-10-03T21:31:16";
	
	var result = await context.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
				.Where(c => c.SensorId == "sensor-1")
				.Where(c => Bounds.WindowStart > windowStart && Bounds.WindowEnd <= windowEnd)
				.FirstOrDefaultAsync();

	Console.WriteLine($"{result?.SensorId} - {result?.AvgValue}");

	string ksql = $"SELECT * FROM {MaterializedViewName} WHERE SensorId = 'sensor-1';";
	result = await context.ExecutePullQuery<IoTSensorStats>(ksql);

	"Finished".Dump();
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