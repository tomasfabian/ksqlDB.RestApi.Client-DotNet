using System.ComponentModel.DataAnnotations;

namespace Kafka.DotNet.SqlServer.Tests.Data
{
  public record IoTSensor
  {
    [Key]
    public string SensorId { get; set; }
    public int Value { get; set; }
  }
}