using System.Text.Json.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Streams;

public record Stream
{
  /// <summary>
  /// The source type. Always returns STREAM.
  /// </summary>
  [JsonPropertyName("type")]
  public string Type { get; set; } = null!;

  /// <summary>
  /// The name of the stream.
  /// </summary>
  [JsonPropertyName("name")]
  public string Name { get; set; } = null!;

  /// <summary>
  /// The topic backing the stream.
  /// </summary>
  [JsonPropertyName("topic")]
  public string Topic { get; set; } = null!;

  /// <summary>
  /// The serialization format of the key in the stream. One of JSON, AVRO, PROTOBUF, or DELIMITED.
  /// </summary>
  [JsonPropertyName("keyFormat")]
  public string KeyFormat { get; set; } = null!;

  /// <summary>
  /// The serialization format of the data in the stream. One of JSON, AVRO, PROTOBUF, or DELIMITED.
  /// </summary>
  [JsonPropertyName("valueFormat")]
  public string ValueFormat { get; set; } = null!;

  [JsonPropertyName("isWindowed")]
  public bool IsWindowed { get; set; }
}
