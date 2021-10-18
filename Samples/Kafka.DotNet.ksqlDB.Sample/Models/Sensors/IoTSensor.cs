namespace ksqlDB.Api.Client.Samples.Models.Sensors
{
  public record IoTSensor
  {
    public string SensorId { get; set; }
    public int Value { get; set; }
  }
}