using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;

#nullable enable
public record TablesResponse : StatementResponseBase
{
  [JsonPropertyName("tables")]
  public Table[]? Tables { get; set; }
}
