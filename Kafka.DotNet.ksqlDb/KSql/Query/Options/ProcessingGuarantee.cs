namespace Kafka.DotNet.ksqlDB.KSql.Query.Options
{
  public enum ProcessingGuarantee
  {
    /// <summary>
    /// Records are processed once. To achieve a true exactly-once system, end consumers and producers must also implement exactly-once semantics.
    /// processing.guarantee="exactly_once"
    /// </summary>
    ExactlyOnce,
    /// <summary>
    /// Records are never lost but may be redelivered.
    /// processing.guarantee="at_least_once" 
    /// </summary>
    AtLeastOnce
  }
}