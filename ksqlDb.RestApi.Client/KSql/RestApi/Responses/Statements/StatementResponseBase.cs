using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

public record StatementResponseBase
{
  [JsonPropertyName("error_code")]
  public int ErrorCode { get; set; }

  public string? Message { get; set; }

  [JsonPropertyName("@type")]
  public string Type { get; set; } = null!;

  /// <summary>
  /// The SQL statement whose result is being returned.
  /// </summary>
  public string? StatementText { get; set; }

  /// <summary>
  /// A list of warnings about conditions that may be unexpected by the user, but don't result in failure to execute the statement.
  /// </summary>
  public string[]? Warnings { get; set; }
}
