`InsideOut.sln` showcases a .NET client API example that enables message **production and consumption** from Kafka topics, along with the execution of `ksqlDB` push queries and views. These functionalities are implemented using the `ksqlDB.RestApi.Client`.

`KafkaConsumer` and `KafkaProducer` are based on [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet)

### Blazor Sample

**Blazor Server-side** is a web development framework that allows developers to build interactive web applications using .NET.
It enables the execution of server-side code to handle user interactions and update the UI dynamically without requiring JavaScript.

On the other hand, `ksqlDB` is a streaming database built on Apache Kafka that allows developers to process and analyze real-time streaming data using SQL-like queries.
It provides capabilities for stream processing, and data querying.

When used in conjunction, Blazor Server-side can be integrated with `ksqlDB` to create real-time interactive applications that leverage the power of streaming data processing.

Blazor Server-side, as a web development framework, does not directly expose or provide direct access to the internal workings of a Kafka broker or `ksqlDB`.

Set `docker-compose.csproj` as startup project in `InsideOut.sln`.

### Nuget
```
Install-Package ksqlDB.Api.Client
```

# KafkaProducer (v0.1.0)

A **Kafka producer** is a client application or component that publishes or sends data to Kafka topics.
It is responsible for producing messages or events and making them available to be consumed by Kafka consumers.

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

A **Kafka consumer** is a client application that reads data from Kafka topics and processes it in real-time.
It is responsible for subscribing to one or more Kafka topics and consuming the messages or events published to those topics by Kafka producers.

```
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

### Create a materialized view with ksqlDb.RestApi.Client

In `ksqlDB`, a **materialized view** is a persistent and continuously updated result of a query or transformation applied to one or more Kafka topics.
Materialized views in `ksqlDB` provide a powerful mechanism for building real-time data pipelines, stream processing, and real-time analytics applications.

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

The provided code snippet demonstrates the creation of a `ksqlDB` stream using the `KSqlDbRestApiClient` class.

First, an instance of the `HttpClientFactory` class is created, specifying the base URL for the `ksqlDB` server as the constructor parameter. This will be used for making HTTP requests to the `ksqlDB` server.

Next, a `KSqlDbRestApiClient` object is instantiated, passing the `HttpClientFactory` instance as the constructor argument. This client will handle the communication with the `ksqlDB` server.

The code then calls the `CreateStreamAsync` method on the restApiClient object to create a `ksqlDB` stream.

The `ifNotExists` parameter is a boolean flag that indicates whether the stream should only be created if it doesn't already exist.

Finally, the code awaits the `CreateStreamAsync` method, which returns an `HttpResponseMessage`. This allows for handling the response from the `ksqlDB` server, such as checking for success or handling any errors or exceptions that may occur during the stream creation process.

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

In summary, the provided code creates a `ksqlDB` stream using the `KSqlDbRestApiClient` class.
It utilizes an `HttpClientFactory` to establish a connection to the `ksqlDB` server, and then calls the `CreateStreamAsync` method on the client, passing the stream metadata and the `ifNotExists` flag to specify the desired behavior.

### Serialization

The following code overrides the `CreateDeserializer` method, which is responsible for creating a deserializer for the `IoTSensorStats` type.
In this case, the code uses the `KafkaDataContractJsonDeserializer` class to create a deserializer that can handle JSON data and convert it into `IoTSensorStats` objects.

By implementing the `CreateDeserializer` method, the code customizes the deserialization process for the `IoTSensorStats` type in the consumer.
This allows the consumer to properly deserialize and process messages received from the Kafka topic, ensuring compatibility between the JSON data and the `IoTSensorStats` class.

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

# Intercepting a consumer construction

The provided code represents a custom consumer implementation named "SensorsTableConsumer" that extends the KafkaConsumer class. 

This code intercepts the Kafka consumer builder for the "SensorsTableConsumer" and modifies it to set the offset end based on the last consumed offset plus 1 or the beginning offset, providing control over where the consumer starts consuming messages from the Kafka topic.

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
The provided code demonstrates a cleanup process in `ksqlDB`.
It involves executing commands to drop tables and streams, as well as deleting associated topics.

```KSQL
drop table SENSORSTABLE delete topic;
drop stream SENSORSSTREAM delete topic;
drop stream IOTSENSORS delete topic;
```

### Linqpad example
[Kafka streaming](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/Samples/ksqlDB.RestApi.Client.LinqPad/ksqldb.Streaming.linq)

# Acknowledgements:
- [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka/)
- [System.Reactive](https://www.nuget.org/packages/System.Reactive/)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json/)
