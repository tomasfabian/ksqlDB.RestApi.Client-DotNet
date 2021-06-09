Kafka.DotNet.InsideOut is a client API for producing and consuming kafka topics and ksqlDB push queries and views

### Blazor Sample 
Set docker-compose.csproj as startup project in Kafka.DotNet.ksqlDb.Experimental.sln.

### Nuget
```
Install-Package Kafka.DotNet.InsideOut
```

# KafkaProducer (v0.1.0)

```C#
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Producer;

const string bootstrapServers = "localhost:29092";

private async Task ProduceValueAsync()
{
	var producerConfig = new ProducerConfig
	{
		BootstrapServers = bootstrapServers,
		Acks = Acks.Leader
	};

	using var kafkaProducer = new KafkaProducer<int, IoTSensor>("IotSensors", producerConfig);
	
	var sensor = new IoTSensor
	{
		SensorId = $"Sensor-1",
		Value = 42
	};

	var deliveryResult = await kafkaProducer.ProduceMessageAsync(1, sensor);
}
```

# KafkaConsumer (v0.1.0)

```C#
using System.Reactive.Linq;
using System.Threading.Tasks;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;

const string bootstrapServers = "localhost:29092";

var consumerConfig = new ConsumerConfig
{
  BootstrapServers = bootstrapServers,
  GroupId = System.Diagnostics.Process.GetCurrentProcess().ProcessName,
  AutoOffsetReset = AutoOffsetReset.Latest
};

var kafkaConsumer = new SensorsTableConsumer(consumerConfig);

var subscription = kafkaConsumer.ConnectToTopicAsync()
  .Take(100)
  .Subscribe(c => Console.WriteLine($"Value: {c.Value}"));
```

```C#

public class SensorsTableConsumer : KafkaConsumer<string, IoTSensorStats>
{
	public SensorsTableConsumer(ConsumerConfig consumerConfig)
		: base("SENSORSTABLE", consumerConfig)
	{
	}
}

```

### Create a materialized view (Kafka.DotNet.ksqlDB)

```C#
using System.Threading.Tasks;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;

private string KsqlDbUrl => "http://localhost:8088";

private async Task CreateOrReplaceMaterializedTableAsync()
{
	await using var context = new KSqlDBContext(KsqlDbUrl);

	var statement = context.CreateOrReplaceTableStatement(tableName: "SensorsTable")
		.As<IoTSensor>("IotSensors")
		.Where(c => c.SensorId != "Sensor-5")
		.GroupBy(c => c.SensorId)
		.Select(c => new { SensorId = c.Key, Count = c.Count(), AvgValue = c.Avg(a => a.Value) });

	var httpResponseMessage = await statement.ExecuteStatementAsync();

	if (!httpResponseMessage.IsSuccessStatusCode)
	{
		var statementResponse = httpResponseMessage.ToStatementResponse();
	}
	else
	{
		var statementResponses = httpResponseMessage.ToStatementResponses();
	}
}
```

### Create a stream (Kafka.DotNet.ksqlDB)

```C#
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.Sample.Data.Sensors;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;

private string KsqlDbUrl => "http://localhost:8088";

private async Task<HttpResponseMessage> TryCreateStreamAsync()
{
	EntityCreationMetadata metadata = new()
	{
		KafkaTopic = "IotSensors",
		Partitions = 1,
		Replicas = 1
	};

	var http = new HttpClientFactory(new Uri(KsqlDbUrl));
	var restApiClient = new KSqlDbRestApiClient(http);

	var httpResponseMessage = await restApiClient.CreateStreamAsync<IoTSensor>(metadata, ifNotExists: true);

	return httpResponseMessage;
}
```

### Serialization
```C#
using System.Runtime.Serialization;

[DataContract]
public record SensorsStream
{
public string Id { get; set; }

[DataMember(Name = "VALUE")]
public int Value { get; set; }
}
```

How to override the default System.Text.Json deserializer:
```C#
using Blazor.Sample.Data.Sensors;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;
using Kafka.DotNet.InsideOut.Serdes;

public class SensorsTableConsumer : KafkaConsumer<string, IoTSensorStats>
{
  public SensorsTableConsumer(ConsumerConfig consumerConfig) 
    : base("SensorsTable", consumerConfig)
  {
  }

  protected override IDeserializer<IoTSensorStats> CreateDeserializer()
  {
    return new KafkaDataContractJsonDeserializer<IoTSensorStats>();
  }
}
```

# Interception of consumer build
How to set offset end:

```C#
using Kafka.DotNet.InsideOut.Consumer.Extensions;

public class SensorsTableConsumer : KafkaConsumer<string, IoTSensorStats>
{
  public SensorsTableConsumer(ConsumerConfig consumerConfig) 
    : base("SensorsTable", consumerConfig)
  {
  }

  protected override void InterceptConsumerBuilder(ConsumerBuilder<string, IoTSensorStats> consumerBuilder)
  {
    base.InterceptConsumerBuilder(consumerBuilder);

    consumerBuilder.SetOffsetEnd(topicPartition => (LastConsumedOffset + 1) ?? Offset.Beginning);
  }
}
```

### Cleanup
```KSQL
drop table SENSORSTABLE delete topic;
drop stream SENSORSSTREAM delete topic;
drop stream IOTSENSORS delete topic;
```