using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Serdes;

namespace Kafka.DotNet.ksqlDB.InsideOut.Producer
{
  public abstract class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
  {
    private readonly ProducerConfig producerConfig;

    public IProducer<TKey, TValue> Producer { get; }

    protected KafkaProducer(ProducerConfig producerConfig)
    {
      this.producerConfig = producerConfig ?? throw new ArgumentNullException(nameof(producerConfig));

      Producer = CreateProducer();
    }

    public abstract string TopicName { get; }

    protected IProducer<TKey, TValue> CreateProducer()
    {
      var producerBuilder = new ProducerBuilder<TKey, TValue>(producerConfig)
        .SetValueSerializer(CreateSerializer());

      InterceptProducerBuilder(producerBuilder);

      return producerBuilder.Build();
    }

    protected IProducer<TKey, Null> CreateNullProducer()
    {
      var nullSerializer = new KafkaJsonSerializer<Null>();

      var producerBuilder = new ProducerBuilder<TKey, Null>(producerConfig)
        .SetValueSerializer(nullSerializer);

      return producerBuilder.Build();
    }

    protected virtual void InterceptProducerBuilder(ProducerBuilder<TKey, TValue> producerBuilder)
    {
    }

    protected virtual ISerializer<TValue> CreateSerializer()
    {
      return new KafkaJsonSerializer<TValue>();
    }

    public async Task<DeliveryResult<TKey, TValue>> ProduceMessageAsync(TKey key, TValue value)
    {
      var message = new Message<TKey, TValue>
      {
        Key = key,
        Value = value,
      };

      var deliveryResult = await Producer.ProduceAsync(TopicName, message);

      return deliveryResult;
    }

    public async Task<DeliveryResult<TKey, TValue>> DeleteMessageAsync(TKey id)
    {
      var deliveryResult = await ProduceMessageAsync(id, default);

      return deliveryResult;
    }

    public void Dispose() => Dispose(true);

    private bool disposed;

    protected void Dispose(bool disposing)
    {
      if (disposed)
        return;

      if (disposing)
      {
        OnDispose();

        using (Producer)
        {
        }
      }

      disposed = true;
    }

    protected virtual void OnDispose()
    {
    }
  }
}