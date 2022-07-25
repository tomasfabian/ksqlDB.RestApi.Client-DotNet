namespace ksqlDB.RestApi.Client.KSql.RestApi.Serialization;

public enum SerializationFormats
{
  None,
  Delimited,
  Json,
  Json_SR,
  Avro,
  Kafka,
  Protobuf,
  // Protobuf No Schema Registry. ksqldb 0.27.1
  Protobuf_NoSR
}