using System.Text;
using Confluent.Kafka;
using InsideOut.Serdes;

namespace InsideOut.Producer;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
{
  private readonly ProducerConfig producerConfig;

  public IProducer<TKey, TValue> Producer { get; }

  public KafkaProducer(string topicName, ProducerConfig producerConfig)
  {
    if(topicName == null)
      throw new ArgumentNullException(nameof(topicName));

    if (topicName.Trim() == String.Empty)
      throw new ArgumentException("Input cannot be empty", nameof(topicName));

    TopicName = topicName;

    this.producerConfig = producerConfig ?? throw new ArgumentNullException(nameof(producerConfig));

    Producer = CreateProducer();
  }

  public string TopicName { get; }

  protected IProducer<TKey, TValue> CreateProducer()
  {
    var producerBuilder = new ProducerBuilder<TKey, TValue>(producerConfig)
      .SetValueSerializer(CreateSerializer());

    InterceptProducerBuilder(producerBuilder);

    return producerBuilder.Build();
  }

  protected virtual void InterceptProducerBuilder(ProducerBuilder<TKey, TValue> producerBuilder)
  {
  }

  protected virtual ISerializer<TValue> CreateSerializer()
  {
    return new KafkaJsonSerializer<TValue>();
  }

  public async Task<DeliveryResult<TKey, TValue>> ProduceMessageAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
  {
    var message = new Message<TKey, TValue>
    {
      Key = key,
      Value = value,
      Headers = new Headers
      {
        new Header("abc", Encoding.UTF8.GetBytes("value"))
      }
    };

    var deliveryResult = await Producer.ProduceAsync(TopicName, message, cancellationToken);

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
