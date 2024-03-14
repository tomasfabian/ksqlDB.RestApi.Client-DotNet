using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Connectors;

public record Connector
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = null!;

  [JsonPropertyName("type")]
  public string Type { get; set; } = null!;

  [JsonPropertyName("className")]
  public string ClassName { get; set; } = null!;

  [JsonPropertyName("state")]
  public string State { get; set; } = null!;
}
