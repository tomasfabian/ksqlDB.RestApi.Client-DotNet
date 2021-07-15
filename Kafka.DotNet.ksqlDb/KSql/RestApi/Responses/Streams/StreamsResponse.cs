using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Streams
{
  public record StreamsResponse : StatementResponseBase
  {
    [JsonPropertyName("streams")]
    public Stream[] Streams { get; set; }
  }
}