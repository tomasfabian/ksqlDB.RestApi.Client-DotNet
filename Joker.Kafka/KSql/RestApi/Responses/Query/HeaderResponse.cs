using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Query
{
  internal class HeaderResponse
  {
    [JsonPropertyName("header")]
    public Header Header { get; set; }
  }
}