using System;

namespace ksqlDB.RestApi.Client.KSql.Query.Options;

public static class AutoOffsetResetExtensions
{
  public static AutoOffsetReset ToAutoOffsetReset(this string autoOffsetResetValue)
  {        
    if (autoOffsetResetValue == "earliest")
      return AutoOffsetReset.Earliest;
        
    if (autoOffsetResetValue == "latest")
      return AutoOffsetReset.Latest;

    throw new ArgumentOutOfRangeException(nameof(autoOffsetResetValue), autoOffsetResetValue, null);
  }

  public static string ToKSqlValue(this AutoOffsetReset value)
  {
    return value.ToString().ToLower();
  }
}