using Blazor.Sample.Data;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Producer;

namespace Blazor.Sample.Kafka.Producers
{
  public class ItemsKafkaProducer : KafkaProducer<int, Item>
  {
    public ItemsKafkaProducer(ProducerConfig producerConfig) 
      : base(producerConfig)
    {
    }

    public override string TopicName { get; } = TopicNames.Items;
  }
}