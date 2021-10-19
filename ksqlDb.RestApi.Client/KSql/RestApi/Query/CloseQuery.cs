using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Query
{
  public record CloseQuery
  {
    [JsonPropertyName("queryId")]
    public string QueryId { get; set; }
  }
}