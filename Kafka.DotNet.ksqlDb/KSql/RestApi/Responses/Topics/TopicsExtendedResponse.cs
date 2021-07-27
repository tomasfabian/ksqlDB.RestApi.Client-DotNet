using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Topics
{
  public record TopicsExtendedResponse : StatementResponseBase
  {
    [JsonPropertyName("topics")]
    public TopicExtended[] Topics { get; set; }
  }
}