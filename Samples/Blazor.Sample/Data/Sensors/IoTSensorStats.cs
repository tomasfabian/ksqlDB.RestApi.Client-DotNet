namespace Blazor.Sample.Data.Sensors
{
  public record IoTSensorStats
  {
    public string SensorId { get; set; }
    public double AvgValue { get; set; }

    public int Count { get; set; }

    public long WindowStart { get; set; }
    public long WindowEnd { get; set; }
  }
}