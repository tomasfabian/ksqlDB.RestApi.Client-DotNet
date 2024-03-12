using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query;

internal class RowResponse
{
  [JsonPropertyName("row")]
  public Row? Row { get; set; }

  [JsonPropertyName("errorMessage")]
  public object? ErrorMessage { get; set; }
}
