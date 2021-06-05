using System;
using System.Threading;
using Confluent.Kafka;

namespace Kafka.DotNet.ksqlDB.InsideOut.Consumer
{
  public interface IKafkaConsumer : IDisposable
  {
    string TopicName { get; }
  }

  public interface IKafkaConsumer<TKey, TValue> : IKafkaConsumer
  {
    IObservable<Message<TKey, TValue>> ConnectToTopicAsync();
  }
}