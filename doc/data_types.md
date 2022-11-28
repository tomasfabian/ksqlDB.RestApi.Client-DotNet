# Data types

# System.GUID as ksqldb VARCHAR type (v2.4.0)

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

public record LinkCreated
{
  public string Name { get; set; }

  [Key]
  public Guid AggregateRootId { get; set; }
}
``` 
```C#
EntityCreationMetadata metadata = new()
{
  KafkaTopic = "MyGuids",
  ValueFormat = SerializationFormats.Json,
  Partitions = 1,
  Replicas = 1
};

var httpResponseMessage = await restApiProvider.CreateStreamAsync<LinkCreated>(metadata, ifNotExists: true)
  .ConfigureAwait(false);
```
KSQL:

```SQL
CREATE STREAM IF NOT EXISTS LINKCREATEDS (
  NAME STRING,
  AGGREGATEROOTID STRING KEY
) WITH (KAFKA_TOPIC='MyGuids', KEY_FORMAT='KAFKA', PARTITIONS='1', REPLICAS='1', VALUE_FORMAT='JSON');
```
