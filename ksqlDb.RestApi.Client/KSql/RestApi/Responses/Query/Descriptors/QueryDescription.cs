namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors
{
  public record QueryDescription
  {
    public string Id { get; set; }
    public string StatementText { get; set; }
    public object WindowType { get; set; }
    public Field[] Fields { get; set; }
    public string[] Sources { get; set; }
    public object[] Sinks { get; set; }
    public string Topology { get; set; }
    public string ExecutionPlan { get; set; }
    public object OverriddenProperties { get; set; }
    public object KsqlHostQueryStatus { get; set; }
    public string QueryType { get; set; }
    public object[] QueryErrors { get; set; }
    public object[] TasksMetadata { get; set; }
    public object State { get; set; }
  }
}