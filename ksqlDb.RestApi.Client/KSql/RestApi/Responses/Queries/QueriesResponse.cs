using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;

public record QueriesResponse : StatementResponseBase
{
  public Query[]? Queries { get; set; }
}
