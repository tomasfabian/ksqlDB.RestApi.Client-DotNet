# Kafka stream processing example

Example of how to consume a table with a kafka consumer. The following code is based on sample named [InsideOut](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/InsideOut)
```
Install-Package ksqlDB.RestApi.Client
```

```C#
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

private async Task CreateOrReplaceMaterializedTableAsync()
{
  string ksqlDbUrl = "http://localhost:8088";

  await using var context = new KSqlDBContext(ksqlDbUrl);

  var statement = context.CreateOrReplaceTableStatement(tableName: "SENSORSTABLE")
    .As<IoTSensor>("IotSensors")
    .Where(c => c.SensorId != "Sensor-5")
    .GroupBy(c => c.SensorId)
    .Select(c => new { SensorId = c.Key, Count = c.Count(), AvgValue = c.Avg(a => a.Value) });

  var httpResponseMessage = await statement.ExecuteStatementAsync();

  if (!httpResponseMessage.IsSuccessStatusCode)
  {
    var statementResponse = httpResponseMessage.ToStatementResponse();
  }
}

public record IoTSensor
{
  [Key]
  public string SensorId { get; set; }
  public int Value { get; set; }
}
```

```C#
public class SensorsTableConsumer : KafkaConsumer<string, IoTSensorStats>
{
  public SensorsTableConsumer(ConsumerConfig consumerConfig)
    : base("SENSORSTABLE", consumerConfig)
  {
  }
}

public record IoTSensorStats
{
  public string SensorId { get; set; }
  public double AvgValue { get; set; }
  public int Count { get; set; }
}
```

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

[Blazor server side example](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet) - InsideOut.sln
