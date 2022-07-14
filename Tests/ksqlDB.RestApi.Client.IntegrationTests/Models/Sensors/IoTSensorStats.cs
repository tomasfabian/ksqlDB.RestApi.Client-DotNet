namespace ksqlDB.Api.Client.IntegrationTests.Models.Sensors;

public record IoTSensorStats
{
  public long WindowStart { get; set; }
  public long WindowEnd { get; set; }

  public string SensorId { get; set; }
  public double AvgValue { get; set; }
}