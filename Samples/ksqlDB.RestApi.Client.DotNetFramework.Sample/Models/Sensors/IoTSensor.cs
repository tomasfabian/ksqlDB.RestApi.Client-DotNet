using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.Api.Client.Samples.Models.Sensors
{
  public record IoTSensor
  {
    public string SensorId { get; set; } = null!;
    public int Value { get; set; }

    [Headers("abc")]
    public byte[] Header { get; set; } = null!;
  }
}