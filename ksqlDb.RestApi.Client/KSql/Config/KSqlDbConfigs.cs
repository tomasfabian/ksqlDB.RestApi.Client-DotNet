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
  /// Determines what to do when there is no initial offset in Apache Kafka® or if the current offset doesn't exist on the server.
  /// The default value in ksqlDB is latest, which means all Kafka topics are read from the latest available offset.
  /// </summary>
  public static readonly string KsqlStreamsAutoOffsetReset = "ksql.streams.auto.offset.reset";

  /// <summary>
  /// The maximum number of records to buffer per partition. The default is 1000.
  /// </summary>
  public static readonly string KsqlStreamsBufferedRecordsPerPartition = "ksql.streams.buffered.records.per.partition";

  /// <summary>
  /// The maximum amount of time a task will idle without processing data when waiting for all of its input partition buffers to contain records.
  /// </summary>
  public static readonly string KsqlStreamsMaxTaskIdleMs = "ksql.streams.max.task.idle.ms";

  /// <summary>
  /// The maximum amount of time, in milliseconds, a task might stall due to internal errors and retries until an error is raised.
  /// </summary>
  public static readonly string KsqlStreamsTaskTimeoutMs = "ksql.streams.task.timeout.ms";

  /// <summary>
  /// Config to enable/disable forwarding pull queries to standby hosts when the active is dead.
  /// </summary>
  public static readonly string KsqlQueryPullEnableStandbyReads = "ksql.query.pull.enable.standby.reads";

  public static readonly string KsqlQueryPullMaxAllowedOffsetLag = "ksql.query.pull.max.allowed.offset.lag";

  /// <summary>
  /// Controls whether pull queries use the interpreter or the code compiler as their expression evaluator.
  /// The default is true.
  /// </summary>
  public static readonly string KSqlQueryPullInterpreterEnabled = "ksql.query.pull.interpreter.enabled";

  /// <summary>
  /// Enable the EMIT FINAL output refinement in a SELECT statement to suppress intermediate results on a windowed aggregation.
  /// The default is true.
  /// </summary>
  public static readonly string KsqlSuppressEnabled = "ksql.suppress.enabled";

  /// <summary>
  /// Configuration constant for enabling continuation tokens for Push V2 queries in KSqlDb.
  /// </summary>
  public static readonly string KsqlQueryPushV2ContinuationTokensEnabled = "ksql.query.push.v2.continuation.tokens.enabled";

  /// <summary>
  /// Configuration constant for setting the processing guarantee property in KSqlDb.
  /// </summary>
  public static readonly string ProcessingGuarantee = "processing.guarantee";
}
