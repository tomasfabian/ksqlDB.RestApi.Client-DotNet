using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Connectors
{
  public record Connector
  {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("className")]
    public string ClassName { get; set; }
    
    [JsonPropertyName("state")]
    public string State { get; set; }
  }
}