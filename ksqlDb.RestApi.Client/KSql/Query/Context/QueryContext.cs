using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

public class QueryContext
{
  internal ModelBuilder ModelBuilder { get; init; } = null!;
  public string? FromItemName { get; internal set; }
  internal AutoOffsetReset? AutoOffsetReset { get; set; }
}
