using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

#nullable enable
public class CommandStatusResponse
{
  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("message")]
  public string? Message { get; set; }

  [JsonPropertyName("queryId")]
  public string? QueryId { get; set; }
}
