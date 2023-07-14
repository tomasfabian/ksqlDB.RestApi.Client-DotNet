namespace ksqlDB.RestApi.Client.KSql.Config;

/// <summary>
/// Contains KSqlDb configuration constants.
/// </summary>
public static class KSqlDbConfigs
{
  /// <summary>
  /// Configuration constant for enabling table scan for pull queries in KSqlDb.
  /// </summary>
  public static readonly string KsqlQueryPullTableScanEnabled = "ksql.query.pull.table.scan.enabled";

  /// <summary>
  /// Configuration constant for enabling headers columns in KSqlDb.
  /// </summary>
  public static readonly string KsqlHeadersColumnsEnabled = "ksql.headers.columns.enabled";

  /// <summary>
  /// Configuration constant for enabling Push V2 queries in KSqlDb.
  /// </summary>
  public static readonly string KsqlQueryPushV2Enabled = "ksql.query.push.v2.enabled";

  /// <summary>
  /// Configuration constant for enabling continuation tokens for Push V2 queries in KSqlDb.
  /// </summary>
  public static readonly string KsqlQueryPushV2ContinuationTokensEnabled = "ksql.query.push.v2.continuation.tokens.enabled";

  /// <summary>
  /// Configuration constant for setting the processing guarantee property in KSqlDb.
  /// </summary>
  public static readonly string ProcessingGuarantee = "processing.guarantee";
}
