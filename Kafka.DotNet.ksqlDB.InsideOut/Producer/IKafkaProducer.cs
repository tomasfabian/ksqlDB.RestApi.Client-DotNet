using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Kafka.DotNet.ksqlDB.InsideOut.Producer
{
  public interface IKafkaProducer : IDisposable
  {
    string TopicName { get; }
  }

  public interface IKafkaProducer<TKey, TValue> : IKafkaProducer
  {
    Task<DeliveryResult<TKey, TValue>> ProduceMessageAsync(TKey key, TValue value);
    Task<DeliveryResult<TKey, TValue>> DeleteMessageAsync(TKey id);
  }
}