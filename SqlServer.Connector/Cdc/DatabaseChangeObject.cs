using SqlServer.Connector.Cdc.Extensions;

namespace SqlServer.Connector.Cdc
{
  /// <summary>
  /// Represents a generic database change object.
  /// </summary>
  /// <typeparam name="TEntity">A generic type parameter TEntity, which represents the entity type associated with the database change.</typeparam>
  public record DatabaseChangeObject<TEntity> : DatabaseChangeObject, IDatabaseChangeObject<TEntity>
  {
    /// <summary>
    /// Represents the state of the entity before the change occurred.
    /// </summary>
    public TEntity Before { get; set; }

    /// <summary>
    /// Represents the state of the entity after the change occurred.
    /// </summary>
    public TEntity After { get; set; }
  }

  /// <summary>
  /// Represents a generic database change object.
  /// </summary>
  public record DatabaseChangeObject
  {
    public Source Source { get; set; }
    // Represents the operation performed on the database (e.g., "insert," "update," or "delete").
    public string Op { get; set; }
    public long TsMs { get; set; }
    public object Transaction { get; set; }

    public ChangeDataCaptureType OperationType => Op.ToChangeDataCaptureType();
  }
}
