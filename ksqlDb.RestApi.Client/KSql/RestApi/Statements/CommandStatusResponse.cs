using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public class CommandStatusResponse
{
  /// <summary>
  /// One of QUEUED, PARSING, EXECUTING, TERMINATED, SUCCESS, or ERROR.
  /// </summary>
  [JsonPropertyName("status")]
  public string? Status { get; set; }

  /// <summary>
  /// Detailed message regarding the status of the execution statement.
  /// </summary>
  [JsonPropertyName("message")]
  public string? Message { get; set; }

  [JsonPropertyName("queryId")]
  public string? QueryId { get; set; }
}
