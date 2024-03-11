using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;

#nullable enable
public record Topic
{
  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("replicaInfo")]
  public int[]? ReplicaInfo { get; set; }
}
