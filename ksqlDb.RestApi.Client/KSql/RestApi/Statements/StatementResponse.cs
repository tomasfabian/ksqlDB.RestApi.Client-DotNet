using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public class StatementResponse
{
  [JsonPropertyName("@type")]
  public string? Type { get; set; }

  [JsonPropertyName("error_code")]
  public int ErrorCode { get; set; }

  [JsonPropertyName("errorMessage")]
  public string? ErrorMessage { get; set; }

  [JsonPropertyName("message")]
  public string? Message { get; set; }

  [JsonPropertyName("statementText")]
  public string? StatementText { get; set; }

  /// <summary>
  /// A string that identifies the requested operation. You can use this ID to poll the result of the operation using the status endpoint.
  /// </summary>
  [JsonPropertyName("commandId")]
  public string? CommandId { get; set; }

  [JsonPropertyName("commandStatus")]
  public CommandStatusResponse? CommandStatus { get; set; }

  /// <summary>
  /// The sequence number of the requested operation in the command queue, or -1 if the operation was unsuccessful.
  /// </summary>
  [JsonPropertyName("commandSequenceNumber")]
  public long CommandSequenceNumber { get; set; }

  /// <summary>
  /// A collection of warnings about conditions that may be unexpected by the user, but don't result in failure to execute the statement.
  /// </summary>
  [JsonPropertyName("warnings")]
  public string[]? Warnings { get; set; }

  /// <summary>
  /// Result objects for statements that were successfully executed by the server.
  /// </summary>
  [JsonPropertyName("entities")]
  public object[]? Entities { get; set; }
}
