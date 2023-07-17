namespace ksqlDB.RestApi.Client.KSql.Query.Functions;

/// <summary>
/// Provides access to KSQL functions.
/// </summary>
public static class KSql
{
  /// <summary>
  /// Gets an instance of KSqlFunctions.
  /// </summary>
  public static KSqlFunctions Functions => F;

  /// <summary>
  /// Gets an instance of KSqlFunctions.
  /// </summary>
  public static KSqlFunctions F => KSqlFunctions.Instance;
}
