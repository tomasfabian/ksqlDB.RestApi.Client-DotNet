using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Query.Descriptors
{
  public record ExplainResponse : StatementResponseBase
  {
    public QueryDescription QueryDescription { get; set; }
  }

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

  public record Field
  {
    public string Name { get; set; }
    public Schema Schema { get; set; }
    public string Type { get; set; }
  }

  public record Schema
  {
    public string Type { get; set; }
    public object Fields { get; set; }
    public object MemberSchema { get; set; }
  }
}