using Blazor.Sample.Data.Sensors;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;

namespace Blazor.Sample.Kafka.Consumers
{
  public class SensorsStreamConsumer : KafkaConsumer<string, SensorsStream>
  {
    public SensorsStreamConsumer(ConsumerConfig consumerConfig) 
      : base(TopicNames.SensorsStream, consumerConfig)
    {
    }
  }
}