namespace ksqlDB.RestApi.Client.KSql.RestApi.Query;

#nullable enable
public record QueryStream<T>
{
  public string? QueryId { get; set; }

  public IAsyncEnumerable<T> EnumerableQuery { get; set; } = null!;
}
