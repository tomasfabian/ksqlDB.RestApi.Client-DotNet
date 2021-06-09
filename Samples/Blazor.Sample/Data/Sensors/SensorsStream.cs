using System.Runtime.Serialization;

namespace Blazor.Sample.Data.Sensors
{
  [DataContract]
  public record SensorsStream
  {
    public string Id { get; set; }

    public int Value { get; set; }
  }
}