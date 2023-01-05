namespace ksqlDB.Api.Client.Tests.Models.Sensors;

public class IoTSensorStats
{
  public string SensorId { get; set; } = null!;
  public double AvgValue { get; set; }
}
