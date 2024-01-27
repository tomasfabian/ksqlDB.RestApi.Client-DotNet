using Confluent.Kafka;

namespace InsideOut.Consumer;

public interface IKafkaConsumer : IDisposable
{
  string TopicName { get; }
}

public interface IKafkaConsumer<TKey, TValue> : IKafkaConsumer
{
  IAsyncEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopicAsync(TimeSpan? timeout, CancellationToken cancellationToken = default);
  IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(CancellationToken cancellationToken = default);
  IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(TimeSpan timeout);
}
