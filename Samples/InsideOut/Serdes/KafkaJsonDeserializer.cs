using System;
using System.Text.Json;
using Confluent.Kafka;

namespace InsideOut.Serdes
{
  public class KafkaJsonDeserializer<TValue> : IDeserializer<TValue>
  {
    public TValue Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {      
      if (isNull)
        return default;

      var jsonSerializerOptions = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      };

      TValue result = JsonSerializer.Deserialize<TValue>(data, jsonSerializerOptions);

      return result;
    }
  }
}