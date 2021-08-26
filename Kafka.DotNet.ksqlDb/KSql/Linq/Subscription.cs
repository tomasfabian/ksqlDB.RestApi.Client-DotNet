namespace Kafka.DotNet.ksqlDB.KSql.Linq
{
  public record Subscription
  {
    public string QueryId { get; internal set; }
  }
}