using Blazor.Sample.Data;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;

namespace Blazor.Sample.KafkaConsumers
{
  public class ItemsKafkaConsumer : KafkaConsumer<int, Item>
  {
    public ItemsKafkaConsumer(ConsumerConfig consumerConfig) 
      : base(consumerConfig)
    {
    }

    public override string TopicName => "ItemsTopic";
  }
}