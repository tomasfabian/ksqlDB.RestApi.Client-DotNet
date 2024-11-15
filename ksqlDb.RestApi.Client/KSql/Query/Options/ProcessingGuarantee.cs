using ksqlDb.RestApi.Client.Infrastructure.Attributes;

namespace ksqlDB.RestApi.Client.KSql.Query.Options;

/// <summary>
/// Specifies the guarantees that Kafka provides regarding message delivery and processing semantics.
/// </summary>
[JsonSnakeCaseStringEnumConverter<ProcessingGuarantee>]
public enum ProcessingGuarantee
{
  /// <summary>
  /// Records are processed once. To achieve a true exactly-once system, end consumers and producers must also implement exactly-once semantics.
  /// processing.guarantee="exactly_once"
  /// </summary>
  ExactlyOnce,

  /// <summary>
  /// Records are processed once. To achieve a true exactly-once system, end consumers and producers must also implement exactly-once semantics.
  /// processing.guarantee="exactly_once_v2"
  /// </summary>
  ///
  ExactlyOnceV2,
  /// <summary>
  /// Records are never lost but may be redelivered.
  /// processing.guarantee="at_least_once"
  /// </summary>
  AtLeastOnce
}
