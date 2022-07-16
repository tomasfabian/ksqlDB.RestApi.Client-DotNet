using System.ComponentModel.DataAnnotations;

namespace Blazor.Sample.Data.Sensors;

public record IoTSensor
{
  [Key]
  public string SensorId { get; set; }
  public int Value { get; set; }
}