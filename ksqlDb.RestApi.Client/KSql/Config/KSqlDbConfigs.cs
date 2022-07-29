namespace ksqlDB.RestApi.Client.KSql.Config;

public static class KSqlDbConfigs
{
  public static readonly string KsqlQueryPullTableScanEnabled = "ksql.query.pull.table.scan.enabled";
  public static readonly string KsqlHeadersColumnsEnabled = "ksql.headers.columns.enabled";
  public static readonly string KsqlQueryPushV2Enabled = "ksql.query.push.v2.enabled";
  public static readonly string KsqlQueryPushV2ContinuationTokensEnabled = "ksql.query.push.v2.continuation.tokens.enabled";

  public static readonly string ProcessingGuarantee = "processing.guarantee";
}