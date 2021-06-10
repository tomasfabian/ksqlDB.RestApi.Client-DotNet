<Query Kind="Program">
  <NuGetReference Prerelease="true">Kafka.DotNet.InsideOut</NuGetReference>
  <NuGetReference Version="1.1.0-rc.1">Kafka.DotNet.ksqlDB</NuGetReference>
  <Namespace>Confluent.Kafka</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Linq.Statements</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.Query.Context</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions</Namespace>
  <Namespace>Kafka.DotNet.ksqlDB.KSql.RestApi.Statements</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Runtime.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Kafka.DotNet.InsideOut.Producer</Namespace>
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
	
	await ProduceValuesAsync();

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

public async Task ProduceValuesAsync()
{
	var producerConfig = new ProducerConfig
	{
		BootstrapServers = bootstrapServers,
		Acks = Acks.Leader
	};

	kafkaProducer = new KafkaProducer<int, IoTSensor>(TopicNames.IotSensors, producerConfig);

	timerSubscription =
		Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(250))
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
	EntityCreationMetadata metadata = new()
	{
		KafkaTopic = TopicNames.IotSensors,
		Partitions = 1,
		Replicas = 1
	};

	var http = new HttpClientFactory(new Uri(KsqlDbUrl));
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

[DataContract]
public record IoTSensorStats
{
	public string SensorId { get; set; }

	[DataMember(Name = "AVGVALUE")]
	public double AvgValue { get; set; }

	[DataMember(Name = "COUNT")]
	public int Count { get; set; }
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
		.Select(c => new { SensorId = c.Key, Count = c.Count(), AvgValue = c.Avg(a => a.Value) });

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