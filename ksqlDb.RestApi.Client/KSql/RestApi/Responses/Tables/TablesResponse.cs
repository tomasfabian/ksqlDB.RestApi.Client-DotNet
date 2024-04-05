using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;

public record TablesResponse : StatementResponseBase
{
  /// <summary>
  /// Collection of tables.
  /// </summary>
  [JsonPropertyName("tables")]
  public Table[]? Tables { get; set; }
}
