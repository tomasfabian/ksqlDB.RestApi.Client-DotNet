using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Streams;

#nullable enable
public record StreamsResponse : StatementResponseBase
{
  [JsonPropertyName("streams")]
  public Stream[]? Streams { get; set; }
}
