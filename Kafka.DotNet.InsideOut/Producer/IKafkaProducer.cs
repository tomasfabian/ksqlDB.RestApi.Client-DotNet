using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Kafka.DotNet.InsideOut.Producer
{
  public interface IKafkaProducer : IDisposable
  {
    string TopicName { get; }
  }

  public interface IKafkaProducer<TKey, TValue> : IKafkaProducer
  {
    Task<DeliveryResult<TKey, TValue>> ProduceMessageAsync(TKey key, TValue value, CancellationToken cancellationToken = default);
    Task<DeliveryResult<TKey, TValue>> DeleteMessageAsync(TKey id);
  }
}