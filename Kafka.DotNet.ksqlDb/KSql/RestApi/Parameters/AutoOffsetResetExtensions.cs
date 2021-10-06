using Kafka.DotNet.ksqlDB.KSql.Query.Options;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters
{
  internal static class AutoOffsetResetExtensions
  {
    internal static AutoOffsetReset ToAutoOffsetReset(this string value)
    {        
      if (value == "earliest")
        return AutoOffsetReset.Earliest;
        
      return AutoOffsetReset.Latest;
    }

    internal static string ToKSqlValue(this AutoOffsetReset value)
    {
      return value.ToString().ToLower();
    }
  }
}