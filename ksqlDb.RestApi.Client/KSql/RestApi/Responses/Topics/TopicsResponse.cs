using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;

public record TopicsResponse : StatementResponseBase
{
  [JsonPropertyName("topics")]
  public Topic[]? Topics { get; set; }
}
