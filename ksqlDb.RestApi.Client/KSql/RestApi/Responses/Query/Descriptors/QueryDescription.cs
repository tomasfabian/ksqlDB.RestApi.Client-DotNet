namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record QueryDescription
{
  /// <summary>
  /// The query ID.
  /// </summary>
  public string Id { get; set; } = null!;

  /// <summary>
  /// The ksqlDB statement for which the query being explained is running.
  /// </summary>
  public string StatementText { get; set; } = null!;

  public object? WindowType { get; set; }

  /// <summary>
  /// An array of field objects that describes each field in the query output.
  /// </summary>
  public Field[] Fields { get; set; } = null!;

  /// <summary>
  /// The streams and tables being read by the query.
  /// </summary>
  public string[] Sources { get; set; } = null!;

  /// <summary>
  /// The streams and tables being written to by the query.
  /// </summary>
  public object[] Sinks { get; set; } = null!;

  /// <summary>
  /// The Kafka Streams topology that the query is running.
  /// </summary>
  public string Topology { get; set; } = null!;

  /// <summary>
  /// The query execution plan.
  /// </summary>
  public string ExecutionPlan { get; set; } = null!;

  /// <summary>
  /// The property overrides that the query is running with.
  /// </summary>
  public object? OverriddenProperties { get; set; }

  public object? KsqlHostQueryStatus { get; set; }

  public string QueryType { get; set; } = null!;

  public object[] QueryErrors { get; set; } = null!;

  public object[] TasksMetadata { get; set; } = null!;

  public object? State { get; set; }
}
