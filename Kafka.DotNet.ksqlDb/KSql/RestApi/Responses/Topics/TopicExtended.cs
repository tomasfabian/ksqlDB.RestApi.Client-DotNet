using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Topics
{
  public record TopicExtended : Topic
  {
    [JsonPropertyName("consumerCount")]
    public int ConsumerCount { get; set; }

    [JsonPropertyName("consumerGroupCount")]
    public int ConsumerGroupCount { get; set; }
  }
}