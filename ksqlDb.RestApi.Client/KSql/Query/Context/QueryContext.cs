using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

#nullable enable
public class QueryContext
{
  public string? FromItemName { get; internal set; }
  internal AutoOffsetReset? AutoOffsetReset { get; set; }
}
