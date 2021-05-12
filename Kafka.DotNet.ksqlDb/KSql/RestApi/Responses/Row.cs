using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses
{
  public class Row
  {
    [JsonPropertyName("columns")]
    public object[] Columns { get; set; }
  }
}