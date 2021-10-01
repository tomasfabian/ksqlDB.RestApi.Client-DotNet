namespace Kafka.DotNet.SqlServer.Cdc
{
  public interface IDatabaseChangeObject
  {
    string Op { get; }
    ChangeDataCaptureType OperationType { get; }
  }

  public interface IDatabaseChangeObject<out TEntity> : IDatabaseChangeObject
  {
    TEntity Before { get; }
    TEntity After { get; }
  }
}