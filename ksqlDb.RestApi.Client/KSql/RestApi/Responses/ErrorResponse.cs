using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses
{
  public class ErrorResponse
  {
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("statementText")]
    public string StatementText { get; set; }

    [JsonPropertyName("entities")]
    public object[] Entities { get; set; }
  }
}