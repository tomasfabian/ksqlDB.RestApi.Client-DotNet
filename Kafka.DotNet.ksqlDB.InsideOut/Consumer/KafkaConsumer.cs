using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Serdes;

namespace Kafka.DotNet.ksqlDB.InsideOut.Consumer
{
  public abstract class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>
  {
    #region Fields

    private readonly ConsumerConfig consumerConfig;

    #endregion

    #region Constructors

    protected KafkaConsumer(ConsumerConfig consumerConfig)
    {
      this.consumerConfig = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));
    }

    #endregion

    #region Properties

    public abstract string TopicName { get; }

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

    private Subject<Message<TKey, TValue>> messagesSubject;
    private CancellationTokenSource cancellationTokenSource;

    public IObservable<Message<TKey, TValue>> ConnectToTopicAsync()
    {
      if (messagesSubject != null)
        return messagesSubject.AsObservable();

      cancellationTokenSource = new CancellationTokenSource();

      messagesSubject = new Subject<Message<TKey, TValue>>();

      Task.Run(() => ConnectToTopic(cancellationTokenSource.Token), cancellationTokenSource.Token);

      return messagesSubject.AsObservable();
    }

    private void ConnectToTopic(CancellationToken cancellationToken)
    {
      using (consumer = CreateConsumer())
      {
        try
        {
          OnConnectToTopic();

          while (!cancellationToken.IsCancellationRequested)
          {
            Consume(cancellationToken);
          }
        }
        finally
        {
          consumer.Close();

          Dispose();
        }
      }
    }

    private void OnConnectToTopic()
    {
      consumer.Subscribe(TopicName);
    }

    private void Consume(CancellationToken cancellationToken)
    {
      var consumeResult = consumer.Consume(cancellationToken);

      ConsumeResult(consumeResult);
    }

    protected Offset? LastConsumedOffset { get; private set; }

    private void ConsumeResult(ConsumeResult<TKey, TValue> consumeResult)
    {
      if (consumeResult == null)
        return;

      messagesSubject?.OnNext(consumeResult.Message);

      LastConsumedOffset = consumeResult.Offset;

      OnConsumeResult(consumeResult);
    }

    protected virtual void OnConsumeResult(ConsumeResult<TKey, TValue> consumeResult)
    {
    }

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
        OnDispose();

        cancellationTokenSource?.Cancel();

        DisposeMessagesSubject();

        LastConsumedOffset = null;
      }

      disposed = true;
    }

    private void DisposeMessagesSubject()
    {
      if (messagesSubject == null) return;
      
      messagesSubject.OnCompleted();

      messagesSubject.Dispose();

      messagesSubject = null;
    }

    protected virtual void OnDispose()
    {
    }

    #endregion
  }
}