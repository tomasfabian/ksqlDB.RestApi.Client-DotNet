using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;

public record Topic
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = null!;

  [JsonPropertyName("replicaInfo")]
  public int[] ReplicaInfo { get; set; } = null!;
}
