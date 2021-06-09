using System.Runtime.Serialization;

namespace Blazor.Sample.Data.Sensors
{
  [DataContract]
  public record IoTSensorStats
  {
    public string SensorId { get; set; }
    [DataMember(Name = "AVGVALUE")]
    public double AvgValue { get; set; }

    [DataMember(Name = "COUNT")]
    public int Count { get; set; }

    public long WindowStart { get; set; }
    public long WindowEnd { get; set; }
  }
}