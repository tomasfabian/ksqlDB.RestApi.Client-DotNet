using System.ComponentModel.DataAnnotations;

namespace SqlServer.Connector.Tests.Data;

public record IoTSensor
{
  [Key]
  public string SensorId { get; set; }
  public int Value { get; set; }
}