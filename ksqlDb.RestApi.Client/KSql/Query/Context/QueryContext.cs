using ksqlDB.RestApi.Client.KSql.Query.Options;

namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  public class QueryContext
  {
    public string FromItemName { get; internal set; }
    internal AutoOffsetReset? AutoOffsetReset { get; set; }
  }
}