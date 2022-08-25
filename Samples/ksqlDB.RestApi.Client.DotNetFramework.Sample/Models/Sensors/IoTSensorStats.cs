namespace ksqlDB.RestApi.Client.DotNetFramework.Sample.Models.Sensors
{
  public record IoTSensorStats
  {
    public string SensorId { get; set; } = null!;
    public double AvgValue { get; set; }

    public long WindowStart { get; set; }
    public long WindowEnd { get; set; }
  }
}