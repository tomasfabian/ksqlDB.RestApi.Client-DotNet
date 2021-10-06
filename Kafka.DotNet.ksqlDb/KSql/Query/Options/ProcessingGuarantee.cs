namespace Kafka.DotNet.ksqlDB.KSql.Query.Options
{
  public enum ProcessingGuarantee
  {
    /// <summary>
    /// exactly_once
    /// </summary>
    ExactlyOnce,
    /// <summary>
    /// at_least_once
    /// </summary>
    AtLeastOnce
  }
}