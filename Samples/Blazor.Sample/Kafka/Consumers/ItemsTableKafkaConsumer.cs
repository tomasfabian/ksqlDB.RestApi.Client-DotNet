using Blazor.Sample.Data;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer.Extensions;

namespace Blazor.Sample.Kafka.Consumers
{
  public class ItemsTableKafkaConsumer : KafkaConsumer<int, ItemTable>
  {
    public ItemsTableKafkaConsumer(ConsumerConfig consumerConfig) 
      : base(consumerConfig)
    {
    }
    
    public override string TopicName { get; } = TopicNames.ItemsTable;

    protected override void InterceptConsumerBuilder(ConsumerBuilder<int, ItemTable> consumerBuilder)
    {
      base.InterceptConsumerBuilder(consumerBuilder);

      consumerBuilder.SetOffsetEnd(topicPartition => (LastConsumedOffset + 1) ?? Offset.Beginning);
    }
  }
}