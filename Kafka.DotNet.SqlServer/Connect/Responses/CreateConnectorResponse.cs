using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kafka.DotNet.SqlServer.Connect.Responses
{
  public record CreateConnectorResponse
  {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("config")]
    public IDictionary<string, string> Config { get; set; }

    [JsonPropertyName("tasks")]
    public ConnectorTask[] Tasks { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
  }
}