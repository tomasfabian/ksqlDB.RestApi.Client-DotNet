using Blazor.Sample.Data.Sensors;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;
using Kafka.DotNet.InsideOut.Consumer.Extensions;

namespace Blazor.Sample.Kafka.Consumers
{
  public class SensorsTableConsumer : KafkaConsumer<string, IoTSensorStats>
  {
    public SensorsTableConsumer(ConsumerConfig consumerConfig) 
      : base(TopicNames.SensorsTable, consumerConfig)
    {
      consumerConfig.Debug += ",consumer";
    }
    
    protected override void InterceptConsumerBuilder(ConsumerBuilder<string, IoTSensorStats> consumerBuilder)
    {
      base.InterceptConsumerBuilder(consumerBuilder);

      consumerBuilder.SetOffsetEnd(topicPartition => (LastConsumedOffset + 1) ?? Offset.Beginning);
    }
  }
}