namespace ksqlDB.RestApi.Client.KSql.Linq
{
  public record Subscription
  {
    public string QueryId { get; internal set; }
  }
}