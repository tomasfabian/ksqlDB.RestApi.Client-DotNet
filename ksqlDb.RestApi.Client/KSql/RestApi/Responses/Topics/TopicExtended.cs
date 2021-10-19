using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics
{
  public record TopicExtended : Topic
  {
    [JsonPropertyName("consumerCount")]
    public int ConsumerCount { get; set; }

    [JsonPropertyName("consumerGroupCount")]
    public int ConsumerGroupCount { get; set; }
  }
}