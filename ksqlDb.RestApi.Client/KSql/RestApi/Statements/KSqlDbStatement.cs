using System.Text;
using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

/// <summary>
/// Defines a sequence of SQL statements.
/// </summary>
public sealed class KSqlDbStatement : QueryParameters
{
  public KSqlDbStatement(string statement)
  {
    if (string.IsNullOrEmpty(statement))
      throw new NullReferenceException(nameof(statement));

    Sql = statement;

    EndpointType = EndpointType.KSql;
  }

  /// <summary>
  /// Dictionary (map) of string variable names and values of any type as initial variable substitution values.
  /// </summary>
  [JsonPropertyName("sessionVariables")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public Dictionary<string, object> SessionVariables { get; set; }

  [JsonIgnore]
  public Encoding ContentEncoding { get; set; } = Encoding.UTF8;

  /// <summary>
  /// Optional. If specified, the statements will not be run until all existing commands up to and including the specified sequence number have completed.
  /// </summary>
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  [JsonPropertyName("commandSequenceNumber")]
  public long? CommandSequenceNumber  { get; set; }
}
