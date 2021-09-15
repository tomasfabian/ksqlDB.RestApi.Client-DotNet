using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Query.Descriptors
{
  public record ExplainResponse : StatementResponseBase
  {
    public QueryDescription QueryDescription { get; set; }
  }
}