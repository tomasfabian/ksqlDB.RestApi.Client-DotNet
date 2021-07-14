using System;
using System.Net;
using System.Text.Json.Serialization;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements
{
  public class StatementResponse
  {
    [JsonPropertyName("@type")]
    public string Type { get; set; }

    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("statementText")]
    public string StatementText { get; set; }

    [JsonPropertyName("commandId")]
    public string CommandId { get; set; }

    [JsonPropertyName("commandStatus")]
    public CommandStatusResponse CommandStatus { get; set; }

    [JsonPropertyName("commandSequenceNumber")]
    public long CommandSequenceNumber { get; set; }

    [JsonPropertyName("warnings")]
    public string[] Warnings { get; set; }

    [JsonPropertyName("entities")]
    public object[] Entities { get; set; }
  }
}