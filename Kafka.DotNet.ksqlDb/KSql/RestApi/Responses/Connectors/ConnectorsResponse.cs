using System.Text.Json.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Connectors
{
  public record ConnectorsResponse : StatementResponseBase
  {
    [JsonPropertyName("connectors")]
    public Connector[] Connectors { get; set; }
  }
}