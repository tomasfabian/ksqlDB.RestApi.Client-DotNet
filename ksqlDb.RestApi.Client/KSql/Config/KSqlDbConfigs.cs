namespace ksqlDB.RestApi.Client.KSql.Config;

public static class KSqlDbConfigs
{
  public static readonly string KsqlQueryPullTableScanEnabled = "ksql.query.pull.table.scan.enabled";
  public static readonly string KsqlHeadersColumnsEnabled = "ksql.headers.columns.enabled";

  public static readonly string ProcessingGuarantee = "processing.guarantee";
}