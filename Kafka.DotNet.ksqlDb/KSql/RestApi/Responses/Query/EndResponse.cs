using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Query
{
  internal class EndResponse
  {
    [JsonPropertyName("finalMessage")]
    public string FinalMessage { get; set; }
  }
}