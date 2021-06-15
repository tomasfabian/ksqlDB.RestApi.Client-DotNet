using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Serdes;

namespace Kafka.DotNet.InsideOut.Consumer
{
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

    public IEnumerable<ConsumeResult<TKey, TValue>> ConnectToTopic(TimeSpan? timeout, CancellationToken cancellationToken)
    {
      using (consumer = CreateConsumer())
      {
        try
        {
          OnConnectToTopic();

          while (!cancellationToken.IsCancellationRequested)
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
          consumer.Close();

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
        OnDispose();

        LastConsumedOffset = null;
      }

      disposed = true;
    }

    protected virtual void OnDispose()
    {
    }

    #endregion
  }
}