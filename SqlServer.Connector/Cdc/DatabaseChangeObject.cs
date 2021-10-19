using SqlServer.Connector.Cdc.Extensions;

namespace SqlServer.Connector.Cdc
{
  public record DatabaseChangeObject<TEntity> : DatabaseChangeObject, IDatabaseChangeObject<TEntity>
  {
    public TEntity Before { get; set; }
    public TEntity After { get; set; }
  }

  public record DatabaseChangeObject
  {
    public Source Source { get; set; }
    public string Op { get; set; }
    public long TsMs { get; set; }
    public object Transaction { get; set; }

    public ChangeDataCaptureType OperationType => Op.ToChangeDataCaptureType();
  }
}