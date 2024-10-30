using System.Text.Json;
using System.Text.Json.Serialization;
using ksqlDb.RestApi.Client.Infrastructure.Attributes;

namespace ksqlDB.RestApi.Client.KSql.Query.Options;

/// <summary>
/// Kafka offset reset policies define how a consumer should handle situations where it tries to read from a partition in Kafka but there is no valid offset available or the offset is out of range.
/// </summary>
[JsonSnakeCaseStringEnumConverter<AutoOffsetReset>]
public enum AutoOffsetReset
{
  /// <summary>
  /// Automatically reset the offset to the earliest available offset.
  /// This will be applied when a consumer group has no committed offset.
  /// </summary>
  Earliest,

  /// <summary>
  /// If the consumer requests an invalid offset or an offset that is out of range, it will reset to the earliest offset available in the partition.
  /// The consumer will then start consuming messages from the beginning of the partition, including any messages that were produced before the consumer started.
  /// </summary>
  Latest
}
