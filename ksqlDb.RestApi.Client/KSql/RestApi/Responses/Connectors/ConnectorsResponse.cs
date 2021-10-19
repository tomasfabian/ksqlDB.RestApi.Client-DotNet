using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Connectors
{
  public record ConnectorsResponse : StatementResponseBase
  {
    [JsonPropertyName("connectors")]
    public Connector[] Connectors { get; set; }
  }
}