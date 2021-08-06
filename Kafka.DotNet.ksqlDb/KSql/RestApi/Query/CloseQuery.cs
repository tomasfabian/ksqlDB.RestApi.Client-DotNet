using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Query
{
  public record CloseQuery
  {
    [JsonPropertyName("queryId")]
    public string QueryId { get; set; }
  }
}