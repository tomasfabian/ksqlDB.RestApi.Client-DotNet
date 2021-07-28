using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Queries
{
  public record StatusCount
  {
    [JsonPropertyName("RUNNING")]
    public int Running { get; set; }
  }
}