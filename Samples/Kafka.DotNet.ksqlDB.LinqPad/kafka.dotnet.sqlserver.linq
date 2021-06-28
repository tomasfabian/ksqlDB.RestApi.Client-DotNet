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
  <Namespace>Kafka.DotNet.SqlServer.Connect</Namespace>
  <RuntimeVersion>5.0</RuntimeVersion>
</Query>

string connectionString = @"Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

string bootstrapServers = "localhost:29092";
string KsqlDbUrl => @"http:\\localhost:8088";

async Task Main()
{
	await CreateSensorsCdcStreamAsync();

	await EnableCdcAsync(tableName);

	await CreateConnectorAsync();

	await using var context = new KSqlDBContext(KsqlDbUrl);

	var semaphoreSlim = new SemaphoreSlim(0, 1);
	
	var cdcSubscription = context.CreateQuery<DatabaseChangeObject<IoTSensor>>("sqlserversensors")
		.WithOffsetResetPolicy(AutoOffsetReset.Latest)
		.Take(5)
		.ToObservable()
		.Subscribe(cdc =>
		{
			var operationType = cdc.OperationType;
			Console.WriteLine(operationType);

			switch (operationType)
			{
				case ChangeDataCaptureType.Created:
					cdc.EntityAfter.Dump("after");
					break;
				case ChangeDataCaptureType.Updated:

					var sensorBefore = cdc.EntityBefore.Dump("before");
					var sensorAfter = cdc.EntityAfter.Dump("after");
					break;
				case ChangeDataCaptureType.Deleted:
					cdc.EntityBefore.Dump("before");
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

	using(cdcSubscription)
	{
	}
}

string tableName = "Sensors";
string schemaName = "dbo";

private async Task EnableCdcAsync(string tableName)
{
	try
	{
		var cdcClient = new Kafka.DotNet.SqlServer.Cdc.CdcClient(connectionString);
		
		await cdcClient.CdcEnableDbAsync();
		await cdcClient.CdcEnableTableAsync(tableName);
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

private async Task CreateConnectorAsync()
{
	var ksqlDbConnect = new KsqlDbConnect(new Uri(KsqlDbUrl));

	SqlServerConnectorMetadata connectorMetadata = CreateConnectorMetadata();

	await ksqlDbConnect.CreateConnectorAsync(connectorName: "MSSQL_SENSORS_CONNECTOR", connectorMetadata);
}

private SqlServerConnectorMetadata CreateConnectorMetadata()
{
	var createConnector = new SqlServerConnectorMetadata(connectionString)
		.SetTableIncludeListPropertyName($"{schemaName}.{tableName}")
		.SetJsonKeyConverter()
		.SetJsonValueConverter()
		.SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
		.SetProperty("database.history.kafka.topic", $"dbhistory.{tableName}")
		.SetProperty("database.server.name", "sqlserver2019")
		.SetProperty("key.converter.schemas.enable", "false")
		.SetProperty("value.converter.schemas.enable", "false")
		.SetProperty("include.schema.changes", "false");

	createConnector.Dump();

	return createConnector as SqlServerConnectorMetadata;
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