using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Connectors
{
  public class ConnectorsResponse
  {
    [JsonPropertyName("@type")]
    public string Type { get; set; }
    
    [JsonPropertyName("statementText")]
    public string StatementText { get; set; }
    
    [JsonPropertyName("warnings")]
    public string[] Warnings { get; set; }
    
    [JsonPropertyName("connectors")]
    public Connector[] Connectors { get; set; }
  }
}