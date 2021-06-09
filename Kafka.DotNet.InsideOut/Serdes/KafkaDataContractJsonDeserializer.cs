using System;
using Confluent.Kafka;

namespace Kafka.DotNet.InsideOut.Serdes
{
  public class KafkaDataContractJsonDeserializer<TValue> : IDeserializer<TValue>
  {
    public TValue Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {      
      if (isNull)
        return default;

      byte[] dataBytes = data.ToArray();

      TValue result = new DataContractJsonDeserializer<TValue>().Deserialize(dataBytes);

      return result;
    }
  }
}