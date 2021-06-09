using System;
using System.Linq;
using Confluent.Kafka;

namespace Kafka.DotNet.InsideOut.Consumer.Extensions
{
  public static class ConsumerBuilderExtensions
  {
    public static ConsumerBuilder<TKey, TValue> SetOffsetEnd<TKey, TValue>(this ConsumerBuilder<TKey, TValue> consumerBuilder, Func<TopicPartition, Offset> getOffset)
    {
      consumerBuilder
        .SetPartitionsAssignedHandler((c, partitions) =>
        {
          var offsets = partitions.Select(topicPartition =>
          {
            var offset = getOffset(topicPartition);

            return new TopicPartitionOffset(topicPartition, offset);
          });

          return offsets;
        });

      return consumerBuilder;
    }
  }
}