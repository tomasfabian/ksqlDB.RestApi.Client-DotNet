<Query Kind="Program">
  <NuGetReference Version="0.1.0" Prerelease="true">Kafka.DotNet.InsideOut</NuGetReference>
  <NuGetReference>ksqlDB.RestApi.Client</NuGetReference>
  <Namespace>Confluent.Kafka</Namespace>
  <Namespace>Kafka.DotNet.InsideOut.Producer</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Linq.Statements</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.Query.Context</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Extensions</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Statements</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Runtime.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>ksqlDB.RestApi.Client.KSql.RestApi.Http</Namespace>
</Query>

const string bootstrapServers = "localhost:29092";

async Task Main()
{
	var consumerConfig = new ConsumerConfig
	{
		BootstrapServers = bootstrapServers,
		GroupId = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
		AutoOffsetReset = AutoOffsetReset.Latest
	};

	await TryCreateStreamAsync();
	
	await CreateOrReplaceMaterializedTableAsync();
	
	var kafkaConsumer = new SensorsTableConsumer(consumerConfig);
	
	ProduceValues();

	var subscription = kafkaConsumer.ConnectToTopicAsync()
		.Take(100)
		.Subscribe(c => c.Dump(), onError: error =>
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
	
	using (subscription)
	using (timerSubscription)
	using (kafkaProducer)
	using (kafkaConsumer)
	{ }
}

SemaphoreSlim semaphoreSlim = new(0, 1);

private IDisposable timerSubscription;
private readonly Random randomValue = new(10);
private readonly Random randomKey = new(1);

private IKafkaProducer<int, IoTSensor> kafkaProducer;

public void ProduceValues()
{
	var producerConfig = new ProducerConfig
	{
		BootstrapServers = bootstrapServers,
		Acks = Acks.Leader
	};

	kafkaProducer = new KafkaProducer<int, IoTSensor>(TopicNames.IotSensors, producerConfig);

	timerSubscription =
		Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3))
			.Subscribe(async _ =>
			{
				int key = randomKey.Next(1, 10);
				int value = randomValue.Next(1, 100);

				var sensor = new IoTSensor
				{
					SensorId = $"Sensor-{key}",
					Value = value
				};

				var deliveryResult = await kafkaProducer.ProduceMessageAsync(key, sensor);
			});
}

private string KsqlDbUrl => "http://localhost:8088";

private async Task<HttpResponseMessage> TryCreateStreamAsync()
{
	EntityCreationMetadata metadata = new(kafkaTopic: TopicNames.IotSensors)
	{
		Partitions = 1,
		Replicas = 1
	};

	var httpClient = new HttpClient
	{
		BaseAddress = new Uri(KsqlDbUrl)
	};
	
	var http = new HttpClientFactory(httpClient);
	var restApiClient = new KSqlDbRestApiClient(http);

	var httpResponseMessage = await restApiClient.CreateStreamAsync<IoTSensor>(metadata, ifNotExists: true);

	return httpResponseMessage;
}

public class SensorsTableConsumer : Kafka.DotNet.InsideOut.Consumer.KafkaConsumer<string, IoTSensorStats>
{
	public SensorsTableConsumer(ConsumerConfig consumerConfig)
		: base(TopicNames.SensorsTable, consumerConfig)
	{
	}
}

public static class TopicNames
{
	public static string IotSensors => "IoTSensors";

	public static string SensorsStream => "SensorsStream".ToUpper();
	public static string SensorsTable => "SensorsTable".ToUpper();
}

public record IoTSensorStats
{
	public string SensorId { get; set; }

	public int Count { get; set; }

	public double Sum { get; set; }

	public int[] LatestByOffset { get; set; }

	public long WindowStart { get; set; }
	public long WindowEnd { get; set; }

	public string LatestByOffsetJoined => LatestByOffset != null ? string.Join(',', LatestByOffset) : string.Empty;
}

public record IoTSensor
{
	public string SensorId { get; set; }
	public int Value { get; set; }
}

private async Task CreateOrReplaceMaterializedTableAsync()
{
	await using var context = new KSqlDBContext(KsqlDbUrl);

	var statement = context.CreateOrReplaceTableStatement(tableName: TopicNames.SensorsTable)
		.As<IoTSensor>(TopicNames.IotSensors)
		.Where(c => c.SensorId != "Sensor-5")
		.GroupBy(c => c.SensorId)
		.Select(c => new { SensorId = c.Key, Count = c.Count(), Sum = c.Sum(a => a.Value), LatestByOffset = c.LatestByOffset(a => a.Value, 2) });

	var httpResponseMessage = await statement.ExecuteStatementAsync();

	if (!httpResponseMessage.IsSuccessStatusCode)
	{
		var statementResponse = httpResponseMessage.ToStatementResponse();
		
		statementResponse.Message.Dump();
	}
	else
	{
		var statementResponses = httpResponseMessage.ToStatementResponses();
	}
}