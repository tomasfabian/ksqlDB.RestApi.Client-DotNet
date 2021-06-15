using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;

namespace Kafka.DotNet.InsideOut.Consumer
{
  public interface IKafkaConsumer : IDisposable
  {
    string TopicName { get; }
  }

  public interface IKafkaConsumer<TKey, TValue> : IKafkaConsumer
  {
    public IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(TimeSpan? timeout, CancellationToken cancellationToken);
  }
}