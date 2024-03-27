using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Streams;

public record StreamsResponse : StatementResponseBase
{
  /// <summary>
  /// Collection of streams.
  /// </summary>
  [JsonPropertyName("streams")]
  public Stream[]? Streams { get; set; }
}
