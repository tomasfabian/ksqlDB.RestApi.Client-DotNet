namespace ksqlDB.RestApi.Client.KSql.Query.Functions;

public struct Entry<TValue>
{
  public string K { get; set; }
  public TValue V { get; set; }
}