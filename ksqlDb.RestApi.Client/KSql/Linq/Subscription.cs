namespace ksqlDB.RestApi.Client.KSql.Linq;

#nullable enable
public record Subscription
{
  public string? QueryId { get; internal set; }
}
