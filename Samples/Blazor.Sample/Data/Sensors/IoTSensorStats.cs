namespace Blazor.Sample.Data.Sensors
{
  public record IoTSensorStats
  {
    public string SensorId { get; set; }

    public int Count { get; set; }

    public double Sum { get; set; }

    public int[] LatestByOffset { get; set; }

    public long WindowStart { get; set; }
    public long WindowEnd { get; set; }

    public string LatestByOffsetJoined => LatestByOffset != null ? string.Join(',', LatestByOffset) : string.Empty;
  }
}