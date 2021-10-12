using Kafka.DotNet.ksqlDB.Sample.Models.Sensors;

namespace Kafka.DotNet.ksqlDB.Sample.Models
{
  public record IoTSensorChange : DatabaseChangeObject<IoTSensor>
  {
  }

  public record DatabaseChangeObject<TEntity> : DatabaseChangeObject
  {
    public TEntity Before { get; set; }
    public TEntity After { get; set; }
    public TEntity EntityBefore => Before;
    public TEntity EntityAfter => After;
  }

  public record DatabaseChangeObject
  {
    public string Op { get; set; }
    public long? TsMs { get; set; }
  }
}