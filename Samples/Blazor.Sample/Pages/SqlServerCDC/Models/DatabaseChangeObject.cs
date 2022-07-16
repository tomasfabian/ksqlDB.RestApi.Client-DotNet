using Blazor.Sample.Data.Sensors;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Cdc.Extensions;

namespace Blazor.Sample.Pages.SqlServerCDC.Models;

public record IoTSensorChange : DatabaseChangeObject<IoTSensor>
{
}

public record IoTSensorRawChange : RawDatabaseChangeObject<IoTSensor>, IDbRecord<IoTSensor>
{
}

public interface IDbRecord
{
  string Op { get; }
  ChangeDataCaptureType OperationType { get; }
}

public interface IDbRecord<out TEntity> : IDbRecord
{
  TEntity EntityBefore { get; }
  TEntity EntityAfter { get; }
}

public record DatabaseChangeObject<TEntity> : DatabaseChangeObject, IDbRecord<TEntity>
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

  public ChangeDataCaptureType OperationType => Op.ToChangeDataCaptureType();
}