# KSqlDbRestApiClient

### Stream and table properties KEY_SCHEMA_ID and VALUE_SCHEMA_ID 
**v1.6.0** (ksqldb v0.24.0)

KEY_SCHEMA_ID - The schema ID of the key schema in Schema Registry. The schema is used for schema inference and data serialization.
VALUE_SCHEMA_ID - The schema ID of the value schema in Schema Registry. The schema is used for schema inference and data serialization.

```C#
EntityCreationMetadata metadata2 = new()
{
  KafkaTopic = "tweets",
  Partitions = 1,
  Replicas = 3,
  KeySchemaId = 1,
  ValueSchemaId = 2
};
```

Generated KSQL statement:

```
 WITH ( KAFKA_TOPIC='tweets', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='3', KEY_SCHEMA_ID=1, VALUE_SCHEMA_ID=2 )
```

### InsertProperties.IncludeReadOnlyProperties
**v1.3.1**

- Inserts - include readonly properties configuration

The initial convention is that all writeable public instance properties and fields are taken into account during the Insert into statement generation.

```C#
public record Foo
{
  public Foo(string name)
  {
    Name = name;
  }

  public string Name { get; }
  public int Count { get; set; }
}
```

```C#
var insertProperties = new InsertProperties
                       {
                         IncludeReadOnlyProperties = true
                       };

await using KSqlDBContext context = new KSqlDBContext(@"http:\\localhost:8088");

var model = new Foo("Bar") {
  Count = 3
};

context.Add(model, insertProperties);

var responseMessage = await context.SaveChangesAsync();
```

### IKSqlDbRestApiClient CreateSourceStreamAsync and CreateSourceTableAsync
**v1.4.0**

- CreateSourceStreamAsync - creates a read-only stream
- CreateSourceTableAsync - creates a read-only table

```C#
string entityName = nameof(IoTSensor;

var metadata = new EntityCreationMetadata(entityName, 1)
               {
                 EntityName = entityName
               };

var httpResponseMessage = await restApiClient.CreateSourceTableAsync<IoTSensor>(metadata, ifNotExists: true);
```

### Rename stream or table column names with the `JsonPropertyNameAttribute`
**v2.2.0**

In cases when you need to use a different name for the C# representation of your ksqldb stream/table column names you can use the `JsonPropertyNameAttribute`:

```C#
using System.Text.Json.Serialization;

internal record Data
{
  [JsonPropertyName("data_id")]
  public string DataId { get; set; }
}
```

```C#
var creationMetadata = new EntityCreationMetadata()
{
  KafkaTopic = "data_values",
  Partitions = 1,
  Replicas = 1,
};

string statement = StatementGenerator.CreateOrReplaceStream<Data>(creationMetadata);
```

```SQL
CREATE OR REPLACE STREAM Data (
	data_id VARCHAR
) WITH ( KAFKA_TOPIC='data_values', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );
```

### Pause and resume persistent qeries (v2.5.0)
`PausePersistentQueryAsync` - Pause a persistent query.
`ResumePersistentQueryAsync` - Resume a paused persistent query.

```C#
private static async Task TerminatePersistentQueryAsync(IKSqlDbRestApiClient restApiClient)
{
  string topicName = "moviesByTitle";

  var queries = await restApiClient.GetQueriesAsync();

  var query = queries.SelectMany(c => c.Queries).FirstOrDefault(c => c.SinkKafkaTopics.Contains(topicName));

  var response = await restApiClient.PausePersistentQueryAsync(query.Id);
  response = await restApiClient.ResumePersistentQueryAsync(query.Id);
  response = await restApiClient.TerminatePersistentQueryAsync(query.Id);
}
```


### Added support for extracting field names and values (for insert and select statements)
**v2.4.0**

```C#
internal record Update
{
  public string ExtraField = "Test value";
}
```

### InsertProperties.UseInstanceType

**v2.4.0**

UseInstanceType set to true will include the public fields and properties from the instance type for the insert statements.

```C#
IMyUpdate value = new MyUpdate
{
  Field = "Value",
  Field2 = "Value2",
};

var insertProperties = new InsertProperties
{
  EntityName = nameof(MyUpdate),
  ShouldPluralizeEntityName = false,
  UseInstanceType = true
};

string statement = new CreateInsert().Generate(value, insertProperties);
```

```C#
private interface IMyUpdate
{
  public string Field { get; set; }
}

private record MyUpdate : IMyUpdate
{
  public string ExtraField = "Test value";
  public string Field { get; set; }
  public string Field2 { get; init; }
}
```

```
INSERT INTO MyUpdate (Field, Field2, ExtraField) VALUES ('Value', 'Value2', 'Test value');
```

### IKSqlDbRestApiClient.AssertTopicExistsAsync and IKSqlDbRestApiClient.AssertTopicNotExistsAsync
**v2.3.0**

[Assert Topic](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/assert-topic/) - asserts that a topic exists or does not exist.

```C#
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;

private static async Task AssertTopicsAsync(IKSqlDbRestApiClient restApiClient)
{
  string topicName = "tweetsByTitle";

  var topicProperties = new Dictionary<string, string>
  {
    { "replicas", "3" },
    { "partitions", "1" },
  };

  var options = new AssertTopicOptions(topicName)
  {
    Properties = topicProperties,
    Timeout = Duration.OfSeconds(3)
  };

  var responses = await restApiClient.AssertTopicNotExistsAsync(options);

  Console.WriteLine(responses[0].Exists);

  responses = await restApiClient.AssertTopicExistsAsync(options);
}
```

```SQL
ASSERT NOT EXISTS TOPIC tweetsByTitle WITH ( replicas=3, partitions=1 ) 3 SECONDS;
ASSERT TOPIC tweetsByTitle WITH ( replicas=3, partitions=1 ) 3 SECONDS;
``` 

### IKSqlDbRestApiClient.AssertSchemaExistsAsync and IKSqlDbRestApiClient.AssertSchemaNotExistsAsync
**v2.3.0**

[Assert Schema](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/assert-schema/)

```C#
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;

private static async Task AssertSchemaAsync(IKSqlDbRestApiClient restApiClient)
{
  string subject = "Kafka-key";
  int id = 21;

  var options = new AssertSchemaOptions(subject, id)
  {
    Timeout = Duration.OfSeconds(3)
  };

  var responses = await restApiClient.AssertSchemaNotExistsAsync(options);

  Console.WriteLine(responses[0].Exists);

  responses = await restApiClient.AssertSchemaExistsAsync(options);
}
```

```SQL
ASSERT NOT EXISTS SCHEMA SUBJECT 'Kafka-key' ID 21 TIMEOUT 3 SECONDS;
ASSERT SCHEMA SUBJECT 'Kafka-key' ID 21 TIMEOUT 3 SECONDS;
```
