using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;

public record Table
{
  /// <summary>
  /// The source type. Always returns TABLE.
  /// </summary>
  [JsonPropertyName("type")]
  public string Type { get; set; } = null!;

  [JsonPropertyName("name")]
  public string Name { get; set; } = null!;

  /// <summary>
  /// The topic associated with the table.
  /// </summary>
  [JsonPropertyName("topic")]
  public string Topic { get; set; } = null!;

  /// <summary>
  /// The serialization format of the key in the table. One of JSON, AVRO, PROTOBUF, or DELIMITED.
  /// </summary>
  [JsonPropertyName("keyFormat")]
  public string KeyFormat { get; set; } = null!;

  /// <summary>
  /// The serialization format of the data in the table. One of JSON, AVRO, PROTOBUF, or DELIMITED.
  /// </summary>
  [JsonPropertyName("valueFormat")]
  public string ValueFormat { get; set; } = null!;

  /// <summary>
  /// True if the table provides windowed results; otherwise, false.
  /// </summary>
  [JsonPropertyName("isWindowed")]
  public bool IsWindowed { get; set; }
}
