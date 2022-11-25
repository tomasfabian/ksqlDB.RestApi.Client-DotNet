using Confluent.Kafka;

namespace InsideOut.Consumer;

public interface IKafkaConsumer : IDisposable
{
  string TopicName { get; }
}

public interface IKafkaConsumer<TKey, TValue> : IKafkaConsumer
{
  IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic();
  IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(TimeSpan timeout);
}
