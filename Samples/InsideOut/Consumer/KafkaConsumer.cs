using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Confluent.Kafka;
using InsideOut.Serdes;
using Microsoft.Extensions.Logging;

namespace InsideOut.Consumer;

public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
{
  #region Fields

  private readonly ConsumerConfig consumerConfig;
  private readonly ILogger logger;

  #endregion

  #region Constructors

  public KafkaConsumer(string topicName, ConsumerConfig consumerConfig, ILogger logger = null)
  {
    if(topicName == null)
      throw new ArgumentNullException(nameof(topicName));

    if (topicName.Trim() == String.Empty)
      throw new ArgumentException("Input cannot be empty", nameof(topicName));

    TopicName = topicName;

    this.consumerConfig = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));
    this.logger = logger;
  }

  public KafkaConsumer(string topicName, ConsumerConfig consumerConfig)
  : this(topicName, consumerConfig, null)
  {
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
    var keyDeserializer = CreateKeyDeserializer();
    var valueDeserializer = CreateValueDeserializer();

    var consumerBuilder = new ConsumerBuilder<TKey, TValue>(consumerConfig);

    if (keyDeserializer != null)
      consumerBuilder.SetKeyDeserializer(keyDeserializer);
    if (valueDeserializer != null)
      consumerBuilder.SetValueDeserializer(valueDeserializer);

    InterceptConsumerBuilder(consumerBuilder);

    return consumerBuilder.Build();
  }

  protected virtual void InterceptConsumerBuilder(ConsumerBuilder<TKey, TValue> consumerBuilder)
  {
  }

  protected virtual IDeserializer<TKey> CreateKeyDeserializer()
  {
    return null;
  }

  protected virtual IDeserializer<TValue> CreateValueDeserializer()
  {
    return new KafkaJsonDeserializer<TValue>();
  }

  public IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(CancellationToken cancellationToken = default)
  {
    return ConnectToTopic(timeout: null, cancellationToken);
  }

  private readonly CancellationTokenSource cancellationTokenSource = new();

  public IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(TimeSpan timeout)
  {
    return ConnectToTopic(timeout, cancellationTokenSource.Token);
  }

  private CancellationTokenSource? linkedCts;

  public async IAsyncEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopicAsync(TimeSpan? timeout, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    var channel = Channel.CreateUnbounded<ConsumeResult<TKey, TValue>>();
    linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    _ = Task.Run(async () =>
    {
      try
      {
        using (consumer = CreateConsumer())
        {
          try
          {
            OnConnectToTopic();

            while (!linkedCts.Token.IsCancellationRequested && !disposed)
            {
              ConsumeResult<TKey, TValue> consumeResult;

              if (timeout.HasValue)
                consumeResult = consumer.Consume(timeout.Value);
              else
                consumeResult = consumer.Consume(linkedCts.Token);

              await channel.Writer.WriteAsync(consumeResult, linkedCts.Token);

              if (consumeResult != null)
                LastConsumedOffset = consumeResult.Offset;
            }
          }
          catch (OperationCanceledException e)
          {
            logger?.LogError(e.Message);
          }
          finally
          {
            Dispose();
          }
        }
      }
      finally
      {
        channel.Writer.Complete();
        await linkedCts.CancelAsync();
      }
    }, linkedCts.Token);


    Console.WriteLine("start");

    while (await channel.Reader.WaitToReadAsync(linkedCts.Token))
    {
      while (channel.Reader.TryRead(out var message))
      {
        yield return message;
      }
    }

    Console.WriteLine("huhuuu");
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
      linkedCts?.Cancel();

      LastConsumedOffset = null;
    }

    disposed = true;
  }

  protected virtual void OnDispose()
  {
  }

  #endregion
}
