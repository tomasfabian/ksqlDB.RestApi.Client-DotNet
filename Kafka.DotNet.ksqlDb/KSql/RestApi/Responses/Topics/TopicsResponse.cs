using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Topics
{
  public record TopicsResponse : StatementResponseBase
  {
    [JsonPropertyName("topics")]
    public Topic[] Topics { get; set; }
  }
}