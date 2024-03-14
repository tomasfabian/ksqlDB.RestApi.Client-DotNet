using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;

public record Table
{
  [JsonPropertyName("type")]
  public string Type { get; set; } = null!;

  [JsonPropertyName("name")]
  public string Name { get; set; } = null!;

  [JsonPropertyName("topic")]
  public string Topic { get; set; } = null!;

  [JsonPropertyName("keyFormat")]
  public string KeyFormat { get; set; } = null!;

  [JsonPropertyName("valueFormat")]
  public string ValueFormat { get; set; } = null!;

  [JsonPropertyName("isWindowed")]
  public bool IsWindowed { get; set; }
}
