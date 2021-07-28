using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Queries
{
  public record QueriesResponse : StatementResponseBase
  {
    [JsonPropertyName("queries")]
    public Query[] Queries { get; set; }
  }
}