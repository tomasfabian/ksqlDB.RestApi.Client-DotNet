using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Tables
{
  public record TablesResponse : StatementResponseBase
  {
    [JsonPropertyName("tables")]
    public Table[] Tables { get; set; }
  }
}