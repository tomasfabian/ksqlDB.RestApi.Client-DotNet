using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

#nullable enable
public record ExplainResponse : StatementResponseBase
{
  public QueryDescription? QueryDescription { get; set; }
}
