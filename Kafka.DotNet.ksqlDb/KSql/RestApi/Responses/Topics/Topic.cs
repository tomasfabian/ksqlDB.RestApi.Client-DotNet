using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Topics
{
  public record Topic
  {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("replicaInfo")]
    public int[] ReplicaInfo { get; set; }
  }
}