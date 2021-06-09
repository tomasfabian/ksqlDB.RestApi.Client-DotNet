using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Kafka.DotNet.ksqlDB.InsideOut.Serdes
{
  public class KafkaJsonSerializer<TValue> : ISerializer<TValue>
  {
    public byte[] Serialize(TValue data, SerializationContext context)
    {
      var json = JsonSerializer.Serialize(data);

      return Encoding.UTF8.GetBytes(json);
    }
  }
}