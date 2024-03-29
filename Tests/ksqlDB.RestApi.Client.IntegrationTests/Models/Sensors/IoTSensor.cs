namespace ksqlDb.RestApi.Client.IntegrationTests.Models.Sensors;

public record IoTSensor
{
  public string SensorId { get; set; } = null!;
  public int Value { get; set; }
}
