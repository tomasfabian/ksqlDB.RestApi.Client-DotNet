namespace ksqlDb.RestApi.Client.Tests.Models.Sensors;

public class IoTSensorStats
{
  public string SensorId { get; set; } = null!;
  public double AvgValue { get; set; }
}
