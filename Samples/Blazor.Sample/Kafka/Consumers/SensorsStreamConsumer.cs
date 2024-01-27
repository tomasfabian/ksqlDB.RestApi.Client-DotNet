using Blazor.Sample.Data.Sensors;
using Confluent.Kafka;
using InsideOut.Consumer;

namespace Blazor.Sample.Kafka.Consumers;

public class SensorsStreamConsumer : KafkaConsumer<string, SensorsStream>
{
  public SensorsStreamConsumer(ConsumerConfig consumerConfig, ILogger<SensorsStreamConsumer> logger)
    : base(TopicNames.SensorsStream, consumerConfig, logger)
  {
    consumerConfig.Debug += ",consumer";
  }

  protected override void InterceptConsumerBuilder(ConsumerBuilder<string, SensorsStream> consumerBuilder)
  {
    consumerBuilder
      .SetPartitionsRevokedHandler((c, partitions) =>
      {
        var remaining = c.Assignment.Where(tp => partitions.All(x => x.TopicPartition != tp));

        var message =
          "** MapWords consumer group partitions revoked: [" +
          string.Join(',', partitions.Select(p => p.Partition.Value)) +
          "], remaining: [" +
          string.Join(',', remaining.Select(p => p.Partition.Value)) +
          "]";

        Console.WriteLine(message);
      })

      .SetPartitionsLostHandler((c, partitions) =>
      {
        var message =
          "** consumer group partitions lost: [" +
          string.Join(',', partitions.Select(p => p.Partition.Value)) +
          "]";
        Console.WriteLine(message);
      })

      .SetPartitionsAssignedHandler((c, partitions) =>
      {
        var message =
          "** consumer group additional partitions assigned: [" +
          string.Join(',', partitions.Select(p => p.Partition.Value)) +
          "], all: [" +
          string.Join(',', c.Assignment.Concat(partitions).Select(p => p.Partition.Value)) +
          "]";

        Console.WriteLine(message);
      });

    base.InterceptConsumerBuilder(consumerBuilder);
  }
}
