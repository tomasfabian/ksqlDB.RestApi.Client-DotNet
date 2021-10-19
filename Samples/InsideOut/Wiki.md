InsideOut is a client API for producing and consuming kafka topics and ksqlDB push queries and views generated with ksqlDB.RestApi.Client or by other means. This package is based on [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet)

### Blazor Sample 
Set docker-compose.csproj as startup project in InsideOut.sln.

### Nuget
```
Install-Package ksqlDB.Api.Client
```

# KafkaProducer (v0.1.0)

```C#
public class SensorsProducer : KafkaProducer<string, IoTSensorStats>
{
  public SensorsProducer(string topicName, ProducerConfig producerConfig) 
    : base(topicName, producerConfig)
  {
  }
    
  protected override void InterceptProducerBuilder(ProducerBuilder<string, IoTSensorStats> producerBuilder)
  {
    base.InterceptProducerBuilder(producerBuilder);
  }
}
```
```C#
public record IoTSensorStats
{
  public string SensorId { get; set; }
  public double AvgValue { get; set; }
  public int Count { get; set; }
}
```
```C#
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Confluent.Kafka;
using InsideOut.Producer;

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

# KafkaConsumer (v1.0.0)
```
Install-Package InsideOut -Version 1.0.0
Install-Package System.Interactive.Async -Version 5.0.0
```

```C#
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Confluent.Kafka;
using InsideOut.Consumer;

const string bootstrapServers = "localhost:29092";

static async Task Main(string[] args)
{
  var consumerConfig = new ConsumerConfig
                       {
                         BootstrapServers = bootstrapServers,
                         GroupId = "Client-01",
                         AutoOffsetReset = AutoOffsetReset.Latest
                       };

  var kafkaConsumer = new KafkaConsumer<string, IoTSensorStats>("IoTSensors", consumerConfig);

  await foreach (var consumeResult in kafkaConsumer.ConnectToTopic().ToAsyncEnumerable().Take(10))
  {
    Console.WriteLine(consumeResult.Message);
  }

  using (kafkaConsumer)
  { }
}
```

# KafkaConsumer (v0.1.0)

```C#
using System.Reactive.Linq;
using System.Threading.Tasks;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Confluent.Kafka;
using InsideOut.Consumer;

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

### Create a materialized view (ksqlDb.RestApi.Client)

```C#
using System.Threading.Tasks;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using ksqlDb.RestApi.Client.KSql.Linq.Statements;
using ksqlDb.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.KSql.RestApi.Extensions;

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

### Create a stream (ksqlDb.RestApi.Client)

```C#
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.Sample.Data.Sensors;
using ksqlDb.RestApi.Client.KSql.RestApi;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements;

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
using InsideOut.Consumer;
using InsideOut.Serdes;

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
using InsideOut.Consumer.Extensions;

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

# Linqpad
[Kafka streaming](https://github.com/tomasfabian/ksqlDb.RestApi.Client/blob/main/Samples/ksqlDb.RestApi.Client.LinqPad/ksqldb.Streaming.linq)

# Acknowledgements:
- [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka/)
- [System.Reactive](https://www.nuget.org/packages/System.Reactive/)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json/)