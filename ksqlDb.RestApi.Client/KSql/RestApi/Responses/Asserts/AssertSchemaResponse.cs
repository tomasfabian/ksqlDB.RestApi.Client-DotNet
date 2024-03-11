using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Responses.Asserts;

#nullable enable
public record AssertSchemaResponse : StatementResponseBase
{
  public string? Subject { get; set; }
  public int? Id { get; set; }
  public bool Exists { get; set; }
}
