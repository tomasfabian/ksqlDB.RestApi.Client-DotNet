using Confluent.Kafka;
using InsideOut.Serdes;

namespace InsideOut.Consumer;

public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
{
  #region Fields

  private readonly ConsumerConfig consumerConfig;

  #endregion

  #region Constructors

  public KafkaConsumer(string topicName, ConsumerConfig consumerConfig)
  {
    if(topicName == null)
      throw new ArgumentNullException(nameof(topicName));

    if (topicName.Trim() == String.Empty)
      throw new ArgumentException("Input cannot be empty", nameof(topicName));

    TopicName = topicName;

    this.consumerConfig = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));
  }

  #endregion

  #region Properties

  public string TopicName { get; }

  private IConsumer<TKey, TValue> consumer;

  protected IConsumer<TKey, TValue> Consumer => consumer;
    
  #endregion
    
  #region Methods

  protected IConsumer<TKey, TValue> CreateConsumer()
  {
    var consumerBuilder = new ConsumerBuilder<TKey, TValue>(consumerConfig)
      .SetValueDeserializer(CreateDeserializer());

    InterceptConsumerBuilder(consumerBuilder);

    return consumerBuilder.Build();
  }

  protected virtual void InterceptConsumerBuilder(ConsumerBuilder<TKey, TValue> consumerBuilder)
  {
  }

  protected virtual IDeserializer<TValue> CreateDeserializer()
  {
    return new KafkaJsonDeserializer<TValue>();
  }

  public IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic()
  {
    return ConnectToTopic(timeout: null, cancellationTokenSource.Token);
  }

  private readonly CancellationTokenSource cancellationTokenSource = new();

  public IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(TimeSpan timeout)
  {
    return ConnectToTopic(timeout, cancellationTokenSource.Token);
  }

  private IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(TimeSpan? timeout, CancellationToken cancellationToken = default)
  {
    if (disposed)
      throw new ObjectDisposedException("Cannot access a disposed object.");

    using (consumer = CreateConsumer())
    {
      try
      {
        OnConnectToTopic();

        while (!cancellationToken.IsCancellationRequested && !disposed)
        {
          ConsumeResult<TKey, TValue> consumeResult;

          if (timeout.HasValue) 
            consumeResult = consumer.Consume(timeout.Value);
          else
            consumeResult = consumer.Consume(cancellationToken);

          yield return consumeResult;

          if(consumeResult != null)
            LastConsumedOffset = consumeResult.Offset;
        }
      }
      finally
      {
        Dispose();
      }
    }
  }

  private void OnConnectToTopic()
  {
    consumer.Subscribe(TopicName);
  }

  protected Offset? LastConsumedOffset { get; private set; }

  private void SeekToEnd(ConsumeResult<TKey, TValue> consumeResult)
  {
    var watermarkOffsets = consumer.GetWatermarkOffsets(consumeResult.TopicPartition);

    consumer.Seek(new TopicPartitionOffset(TopicName, consumeResult.Partition, watermarkOffsets.High));
  }

  protected WatermarkOffsets TryGetOffsets(TopicPartition topicPartition, TimeSpan? timeout = null)
  {
    var offsets = Consumer.QueryWatermarkOffsets(topicPartition, timeout ?? TimeSpan.FromSeconds(1));

    return offsets;
  }

  public void Dispose() => Dispose(true);

  private bool disposed;

  protected void Dispose(bool disposing)
  {
    if (disposed)
      return;

    if (disposing)
    {
      consumer?.Close();

      OnDispose();
        
      cancellationTokenSource.Cancel();

      LastConsumedOffset = null;
    }

    disposed = true;
  }

  protected virtual void OnDispose()
  {
  }

  #endregion
}
