<Query Kind="Program">
  <NuGetReference Prerelease="true">Kafka.DotNet.ksqlDB</NuGetReference>
  <NuGetReference Prerelease="true">Kafka.DotNet.SqlServer</NuGetReference>
  <NuGetReference>System.Data.SqlClient</NuGetReference>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Linq</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Query.Context</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Query.Options</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Generators</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Statements</Namespace>
  <Namespace>Kafka.DotNet.SqlServer.Cdc</Namespace>
  <Namespace>Kafka.DotNet.SqlServer.Cdc.Connectors</Namespace>
  <Namespace>Kafka.DotNet.SqlServer.Cdc.Extensions</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <RuntimeVersion>5.0</RuntimeVersion>
</Query>

string connectionString = @"Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

string bootstrapServers = "localhost:29092";
string KsqlDbUrl => @"http:\\localhost:8088";

ICdcClient CdcProvider { get; set; }

async Task Main()
{
	CdcProvider = new Kafka.DotNet.SqlServer.Cdc.CdcClient(connectionString);

	await CreateSensorsCdcStreamAsync();

	await EnableCdcAsync(tableName);

	await CreateConnectorAsync(tableName);

	await using var context = new KSqlDBContext(KsqlDbUrl);

	var semaphoreSlim = new SemaphoreSlim(0, 1);
	
	var cdcSubscription = context.CreateQuery<DatabaseChangeObject>("sqlserversensors")
		.WithOffsetResetPolicy(AutoOffsetReset.Latest)
		.Take(5)
		.ToObservable()
		.Subscribe(cdc =>
		{
			var operationType = cdc.Op.ToChangeDataCaptureType();
			Console.WriteLine(operationType);

			switch (operationType)
			{
				case ChangeDataCaptureType.Created:
					break;
				case ChangeDataCaptureType.Updated:

					var sensorBefore = System.Text.Json.JsonSerializer.Deserialize<IoTSensor>(cdc.Before).Dump("before");
					var sensorAfter = System.Text.Json.JsonSerializer.Deserialize<IoTSensor>(cdc.After).Dump("after");
					break;
				case ChangeDataCaptureType.Deleted:
					break;
			}
		}, onError: error =>
		{
			semaphoreSlim.Release();
			$"Exception: {error.Message}".Dump("OnError");
		},
		onCompleted: () =>
		{
			semaphoreSlim.Release();
			"Completed".Dump("OnCompleted");
		});

	await semaphoreSlim.WaitAsync();

	using(cdcSubscription){
	}
}

string tableName = "Sensors";
string schemaName = "dbo";

private async Task EnableCdcAsync(string tableName)
{
	try
	{
		await CdcProvider.EnableAsync(tableName);
	}
	catch (Exception e)
	{
		Console.WriteLine(e);
	}
}

private async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
{
	await using var context = new KSqlDBContext(KsqlDbUrl);

	string fromName = "sqlserversensors";
	string kafkaTopic = "sqlserver2019.dbo.Sensors";

	var httpClientFactory = new HttpClientFactory(new Uri(KsqlDbUrl));

	var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

	EntityCreationMetadata metadata = new()
	{
		EntityName = fromName,
		KafkaTopic = kafkaTopic,
		ValueFormat = SerializationFormats.Json,
		Partitions = 1,
		Replicas = 1
	};

	var ksql = StatementGenerator.CreateStream<DatabaseChangeObject>(metadata, ifNotExists: true);

	var httpResponseMessage = await restApiClient.CreateStreamAsync<DatabaseChangeObject>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
		.ConfigureAwait(false);
}

private async Task CreateConnectorAsync(string tableName, string schemaName = "dbo")
{
	var createConnectorStatement = CreateConnector();

	KSqlDbStatement ksqlDbStatement = new(createConnectorStatement);

	var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement).ConfigureAwait(false);
}

private string CreateConnector()
{
	var createConnector = new ConnectorMetadata(connectionString)
		.SetJsonKeyConverter()
		.SetJsonValueConverter()
		.SetTableIncludeListPropertyName($"{schemaName}.{tableName}")
		.SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
		.SetProperty("database.history.kafka.topic", $"dbhistory.{tableName}")
		.SetProperty("database.server.name", "sqlserver2019")
		.SetProperty("key.converter.schemas.enable", "false")
		.SetProperty("value.converter.schemas.enable", "false")
		.SetProperty("include.schema.changes", "false")
		.ToStatement(connectorName: "MSSQL_SENSORS_CONNECTOR");

	createConnector.Dump();

	return createConnector;
}

public Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
{
	var httpClientFactory = new HttpClientFactory(new Uri(KsqlDbUrl));

	var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

	return restApiClient.ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
}

public record IoTSensor
{
	public string SensorId { get; set; }
	public int Value { get; set; }
}