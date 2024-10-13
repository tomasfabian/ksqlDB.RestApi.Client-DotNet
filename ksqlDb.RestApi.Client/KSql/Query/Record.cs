using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.RestApi.Client.KSql.Query;

/// <summary>
/// Represents a Kafka record with pseudo-columns.
/// </summary>
public class Record
{
  /// <summary>
  /// Columns that are populated by the Kafka record's header.
  /// </summary>
  [Ignore]
  [IgnoreByInserts]
  [PseudoColumn]
  [Obsolete("This property will be removed in the future. Headers need to be defined per use case and should have the type ARRAY<STRUCT<key STRING, value BYTES>>.")]
  public string? Headers { get; set; }

  /// <summary>
  /// The offset of the source record.
  /// </summary>
  [Ignore]
  [IgnoreByInserts]
  [PseudoColumn]
  public long? RowOffset { get; set; }

  /// <summary>
  /// The partition of the source record.
  /// </summary>
  [Ignore]
  [IgnoreByInserts]
  [PseudoColumn]
  public short? RowPartition { get; set; }

  /// <summary>
  /// Row timestamp, inferred from the underlying Kafka record if not overridden.
  /// </summary>
  [Ignore]
  [IgnoreByInserts]
  [PseudoColumn]
  public long RowTime { get; set; }
}
