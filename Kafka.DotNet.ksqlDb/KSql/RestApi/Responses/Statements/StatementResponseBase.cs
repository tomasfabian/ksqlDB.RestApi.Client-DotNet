using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Statements
{
  public record StatementResponseBase
  {
    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("@type")]
    public string Type { get; set; }
    
    [JsonPropertyName("statementText")]
    public string StatementText { get; set; }
    
    [JsonPropertyName("warnings")]
    public string[] Warnings { get; set; }
  }
}