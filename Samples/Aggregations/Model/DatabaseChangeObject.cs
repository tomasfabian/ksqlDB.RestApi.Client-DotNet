using Aggregations.Model.Sensors;

namespace Aggregations.Model;

public record IoTSensorChange : DatabaseChangeObject<IoTSensor>
{
}

/// <summary>
/// Represents a generic database change object.
/// </summary>
/// <typeparam name="TEntity">A generic type parameter TEntity, which represents the entity type associated with the database change.</typeparam>
public record DatabaseChangeObject<TEntity> : DatabaseChangeObject
{
  /// <summary>
  /// Represents the state of the entity before the change occurred.
  /// </summary>
  public TEntity? Before { get; set; }
  /// <summary>
  /// Represents the state of the entity after the change occurred.
  /// </summary>
  public TEntity? After { get; set; }
  public TEntity? EntityBefore => Before;
  public TEntity? EntityAfter => After;
}

/// <summary>
/// Represents a generic database change object.
/// </summary>
public record DatabaseChangeObject
{
  // Represents the operation performed on the database (e.g., "insert," "update," or "delete").
  public string Op { get; set; } = null!;
  public long? TsMs { get; set; }
}
