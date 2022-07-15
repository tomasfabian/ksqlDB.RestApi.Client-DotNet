using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public record CreationMetadata
{
  /// <summary>The name of the Kafka topic that backs this stream/table. If this property is not set, then the name of the stream/table in upper case will be used as default.</summary>
  public string KafkaTopic { get; set; }

  /// <summary>Specifies the serialization format of the message key in the topic. If this property is not set, the format from the left-most input stream/table is used.</summary>
  public SerializationFormats? KeyFormat { get; set; }

  /// <summary>Specifies the serialization format of the message value in the topic. If this property is not set, the format from the left-most input stream/table is used.</summary>
  public SerializationFormats? ValueFormat { get; set; }

  /// <summary>Used when VALUE_FORMAT='DELIMITED'. Supports single character to be a delimiter, defaults to ','. For space and tab delimited values you must use the special values 'SPACE' or 'TAB', not an actual space or tab character.</summary>
  public string ValueDelimiter { get; set; }

  /// <summary>	The number of partitions in the backing topic. If this property is not set, then the number of partitions of the input stream/table will be used. In join queries, the property values are taken from the left-most stream or table.</summary>
  public short? Partitions { get; set; }

  /// <summary>The replication factor for the topic. If this property is not set, then the number of replicas of the input stream or table will be used. In join queries, the property values are taken from the left-most stream or table.</summary>
  public short? Replicas { get; set; }

  /// <summary>Sets a column within this stream's schema to be used as the default source of ROWTIME for any downstream queries.</summary>
  public string Timestamp { get; set; }

  /// <summary>Used in conjunction with TIMESTAMP. If not set, ksqlDB timestamp column must be of type bigint. When set, the TIMESTAMP column must be of type varchar and have a format that can be parsed with the Java DateTimeFormatter. If your timestamp format has characters requiring single quotes, you can escape them with two successive single quotes, '', for example: 'yyyy-MM-dd''T''HH:mm:ssX</summary>
  public string TimestampFormat { get; set; }

  public bool? WrapSingleValue { get; set; }

  /// <summary>
  /// The schema specified by the ID will be looked up in the schema registry and used to create the logical schema as well as serialize and deserialize key data.
  /// </summary>
  public int? KeySchemaId { get; set; }
    
  /// <summary>
  /// The schema specified by the ID will be looked up in the schema registry and used to create the logical schema as well as serialize and deserialize value data.
  /// </summary>
  public int? ValueSchemaId { get; set; }

  /// <summary>
  /// 
  /// </summary>
  public string KeySchemaFullName { get; set; }

  /// <summary>
  /// 
  /// </summary>
  public string ValueSchemaFullName { get; set; }
}