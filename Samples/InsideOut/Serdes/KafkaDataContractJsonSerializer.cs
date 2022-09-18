using System.IO;
using System.Runtime.Serialization.Json;
using Confluent.Kafka;

namespace InsideOut.Serdes;

public class KafkaDataContractJsonSerializer<TValue> : ISerializer<TValue>
{
  public byte[] Serialize(TValue data, SerializationContext context)
  {
    var serializer = new DataContractJsonSerializer(typeof(TValue));

    using var memoryStream = new MemoryStream();
      
    serializer.WriteObject(memoryStream, data);

    return memoryStream.ToArray();
  }
}