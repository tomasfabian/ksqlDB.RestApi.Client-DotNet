using System.Text.Json.Serialization;

namespace SqlServer.Connector.Connect
{
  internal record Connector
  {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("config")]
    public IDictionary<string, string> Config { get; set; }
  }
}
