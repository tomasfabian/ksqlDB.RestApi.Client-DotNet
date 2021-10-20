<Query Kind="Program">
  <NuGetReference>Kafka.DotNet.InsideOut</NuGetReference>
  <NuGetReference>ksqlDB.RestApi.Client</NuGetReference>
  <NuGetReference Prerelease="true">SqlServer.Connector</NuGetReference>
  <NuGetReference>System.Data.SqlClient</NuGetReference>
  <Namespace>Confluent.Kafka</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>SqlServer.Connector.Cdc</Namespace>
  <Namespace>Kafka.DotNet.InsideOut.Consumer</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query.Context</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Http</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Serialization</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Generators</Namespace>
  <Namespace>SqlServer.Connector.Connect</Namespace>
  <Namespace>SqlServer.Connector.Cdc.Connectors</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Statements</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Linq</Namespace>
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
  
	//await ConsumeFromTopicAsync(); // Consuming CDC events directly from a Kafka topic
	
	await using var context = new KSqlDBContext(KsqlDbUrl);

	var semaphoreSlim = new SemaphoreSlim(0, 1);
	
	var cdcSubscription = context.CreateQuery<DatabaseChangeObject<IoTSensor>>("sqlserversensors")
		.WithOffsetResetPolicy(ksqlDB.RestApi.Client.KSql.Query.Options.AutoOffsetReset.Latest)
		.Take(5)
		.ToObservable()
		.Subscribe(cdc =>
		{
			var operationType = cdc.OperationType;
			Console.WriteLine(operationType);

			switch (operationType)
			{
				case ChangeDataCaptureType.Created:
					cdc.After.Dump("after");
					break;
				case ChangeDataCaptureType.Updated:

					var sensorBefore = cdc.Before.Dump("before");
					var sensorAfter = cdc.After.Dump("after");
					break;
				case ChangeDataCaptureType.Deleted:
					cdc.Before.Dump("before");
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

async Task ConsumeFromTopicAsync()
{
	string bootstrapServers = "localhost:29092";

	var consumerConfig = new ConsumerConfig
	{
		BootstrapServers = bootstrapServers,
		GroupId = "Client-01",
		AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
	};

	var kafkaConsumer =
		new KafkaConsumer<string, DatabaseChangeObject<IoTSensor>>("sqlserver2019.dbo.Sensors", consumerConfig);

	var dataChanges = kafkaConsumer.ConnectToTopic().ToAsyncEnumerable().Where(c => c.Message.Value.OperationType != ChangeDataCaptureType.Read).Take(2);
	
	await foreach (var consumeResult in dataChanges)
	{
		var message = consumeResult.Message;
		var changeNotification = message.Value; 

		Console.WriteLine(changeNotification.OperationType);
		Console.WriteLine(changeNotification.Before);
		Console.WriteLine(changeNotification.After);
	}

	using (kafkaConsumer)
	{		
	}
}

string tableName = "Sensors";
string schemaName = "dbo";

private async Task EnableCdcAsync(string tableName)
{
	try
	{
		var cdcClient = new CdcClient(connectionString);
		
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

	string fromName = "sqlserversensorsv2";
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

	var ksql = StatementGenerator.CreateStream<DatabaseChangeObject<IoTSensor>>(metadata, ifNotExists: true);

	var httpResponseMessage = await restApiClient.CreateStreamAsync<DatabaseChangeObject<IoTSensor>>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
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