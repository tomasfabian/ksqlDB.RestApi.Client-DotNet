using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query;

internal class HeaderResponse
{
  [JsonPropertyName("header")]
  public Header? Header { get; set; }
}
