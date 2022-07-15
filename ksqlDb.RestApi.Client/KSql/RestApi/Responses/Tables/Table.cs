using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;

public record Table
{
  [JsonPropertyName("type")]
  public string Type { get; set; }

  [JsonPropertyName("name")]
  public string Name { get; set; }
    
  [JsonPropertyName("topic")]
  public string Topic { get; set; }
    
  [JsonPropertyName("keyFormat")]
  public string KeyFormat { get; set; }
    
  [JsonPropertyName("valueFormat")]
  public string ValueFormat { get; set; }
    
  [JsonPropertyName("isWindowed")]
  public bool IsWindowed { get; set; }
}