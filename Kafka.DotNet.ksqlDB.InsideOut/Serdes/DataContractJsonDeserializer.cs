using System.IO;
using System.Runtime.Serialization.Json;

namespace Kafka.DotNet.ksqlDB.InsideOut.Serdes
{
  public class DataContractJsonDeserializer<TValue>
  {
    public TValue Deserialize(byte[] dataBytes)
    {
      var serializer = new DataContractJsonSerializer(typeof(TValue));

      using var memoryStream = new MemoryStream();
      
      memoryStream.Write(dataBytes, 0, dataBytes.Length);
      memoryStream.Position = 0;

      var result = (TValue)serializer.ReadObject(memoryStream);

      return result;
    }
  }
}