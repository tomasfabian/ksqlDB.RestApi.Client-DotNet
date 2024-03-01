using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.RestApi.Client.KSql.Query;

/// <summary>
/// Represents a Kafka record with pseudocolumns.
/// </summary>
public class Record
{
  /// <summary>
  /// Columns that are populated by the Kafka record's header.
  /// </summary>
  [IgnoreByInserts]
  [PseudoColumn]
  public string Headers { get; set; }

  /// <summary>
  /// The offset of the source record.
  /// </summary>
  [IgnoreByInserts]
  [PseudoColumn]
  public long? RowOffset { get; set; }

  /// <summary>
  /// The partition of the source record.
  /// </summary>
  [IgnoreByInserts]
  [PseudoColumn]
  public short? RowPartition { get; set; }

  /// <summary>
  /// Row timestamp, inferred from the underlying Kafka record if not overridden.
  /// </summary>
  [IgnoreByInserts]
  [PseudoColumn]
  public long RowTime { get; set; }
}
