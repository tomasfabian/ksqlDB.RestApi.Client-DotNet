using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Queries
{
  public record QueriesResponse : StatementResponseBase
  {
    public Query[] Queries { get; set; }
  }
}