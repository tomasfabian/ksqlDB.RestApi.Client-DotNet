using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record ExplainResponse : StatementResponseBase
{
  public QueryDescription QueryDescription { get; set; }
}