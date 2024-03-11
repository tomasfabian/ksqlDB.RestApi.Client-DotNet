using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses;

#nullable enable
public class QueryStreamHeader
{
  [JsonPropertyName("queryId")]
  public string? QueryId { get; set; }
    
  [JsonPropertyName("columnNames")]
  public string[] ColumnNames { get; set; } = null!;

  [JsonPropertyName("columnTypes")]
  public string[] ColumnTypes { get; set; } = null!;
}
