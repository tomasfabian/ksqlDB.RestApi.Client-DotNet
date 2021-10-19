using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses
{
  public class Row
  {
    [JsonPropertyName("columns")]
    public object[] Columns { get; set; }
  }
}