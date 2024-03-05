namespace ksqlDB.RestApi.Client.KSql.RestApi.Enums;

/// <summary>
/// Represents different windowing types in ksqlDB that have distinct time boundaries.
/// </summary>
public enum WindowType
{
  /// <summary>
  /// Represents a session window combines records into a session, delineating periods of activity separated by a defined duration of inactivity, known as "idleness".
  /// </summary>
  Session,

  /// <summary>
  /// Represents a hopping window where events are grouped into fixed-size, possibly overlapping, time intervals.
  /// </summary>
  Hopping,

  /// <summary>
  /// Represents a tumbling window where events are grouped into fixed-size, non-overlapping, time intervals.
  /// </summary>
  Tumbling
}
