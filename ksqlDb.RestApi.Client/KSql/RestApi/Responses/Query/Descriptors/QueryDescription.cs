namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record QueryDescription
{
  public string Id { get; set; } = null!;
  public string StatementText { get; set; } = null!;
  public object? WindowType { get; set; }
  public Field[] Fields { get; set; } = null!;
  public string[] Sources { get; set; } = null!;
  public object[] Sinks { get; set; } = null!;
  public string Topology { get; set; } = null!;
  public string ExecutionPlan { get; set; } = null!;
  public object? OverriddenProperties { get; set; }
  public object? KsqlHostQueryStatus { get; set; }
  public string QueryType { get; set; } = null!;
  public object[] QueryErrors { get; set; } = null!;
  public object[] TasksMetadata { get; set; } = null!;
  public object? State { get; set; }
}
