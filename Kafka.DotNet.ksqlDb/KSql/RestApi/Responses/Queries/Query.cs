using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Queries
{
  public record Query
  {
    [JsonPropertyName("queryString")]
    public string QueryString { get; set; }

    [JsonPropertyName("sinks")]
    public string[] Sinks { get; set; }

    [JsonPropertyName("sinkKafkaTopics")]
    public string[] SinkKafkaTopics { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("statusCount")]
    public StatusCount StatusCount { get; set; }
    [JsonPropertyName("queryType")]
    public string QueryType { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
  }
}