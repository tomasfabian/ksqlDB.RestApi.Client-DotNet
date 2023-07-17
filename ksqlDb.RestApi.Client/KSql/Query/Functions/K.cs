namespace ksqlDB.RestApi.Client.KSql.Query.Functions;

/// <summary>
/// Provides access to KSQL functions.
/// </summary>
public static class K
{
  /// <summary>
  /// Gets an instance of KSqlFunctions.
  /// </summary>
  public static KSqlFunctions Functions => KSqlFunctions.Instance;
}
