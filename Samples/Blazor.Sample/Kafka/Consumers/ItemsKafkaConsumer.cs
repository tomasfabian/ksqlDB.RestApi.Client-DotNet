using Blazor.Sample.Data;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;

namespace Blazor.Sample.Kafka.Consumers
{
  public class ItemsKafkaConsumer : KafkaConsumer<int, ItemStream>
  {
    public ItemsKafkaConsumer(ConsumerConfig consumerConfig) 
      : base(consumerConfig)
    {
    }
    
    public override string TopicName { get; } = TopicNames.ItemsStream;
  }
}