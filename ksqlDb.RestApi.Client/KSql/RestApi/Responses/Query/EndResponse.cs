using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query;

internal class EndResponse
{
  [JsonPropertyName("finalMessage")]
  public string? FinalMessage { get; set; }
}
