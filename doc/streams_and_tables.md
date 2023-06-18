# ksqlDB streams and tables

In `ksqlDB`, there are two main types of objects: **streams** and **tables**. In `ksqlDB`, tables are stateful entities, whereas streams are stateless.

Both **streams** and **tables** in `ksqlDB` are defined using a SQL-like syntax and can be queried using standard SQL statements. They provide a declarative way to express the desired computations on the streaming data, enabling real-time processing and analyzying of this data.
The data in streams and tables can be transformed, filtered, joined and aggregated.

## Streams
A stream in `ksqlDB` represents an unbounded sequence of records in `ksqlDB`, where each record is an **immutable** unit of data (fact).
Streams are backed by Kafka topics and inherit their properties.

```SQL
CREATE STREAM users (
  id INT,
  name STRING,
) WITH (
  KAFKA_TOPIC = 'my_topic',
  VALUE_FORMAT = 'JSON'
);
```

In the above example, we are creating a stream named `'users'` with two columns: `'id'` and `'name'`. The `'id'` column is of type **INT**, and `'name'` column is of type **STRING**.

The **KAFKA_TOPIC** configuration specifies the Kafka topic associated with the stream, in this case, `'my_topic'`. The stream will consume events from this Kafka topic.

The **VALUE_FORMAT** configuration indicates the format of the values stored in the Kafka topic. In this example, the values are in JSON format. `ksqlDB` supports various value formats, including JSON, Avro, delimited text, and more.

When you create a stream in `ksqlDB`, it sets up the necessary infrastructure to consume and process events from the specified Kafka topic. The stream continuously reads events from the topic and makes them available for querying and processing in real-time.

The above `ksqlDB` statement can be executed from C# in a more type safe manner:

```C#
private static async Task CreateUsersStreamAsync()
{
  var ksqlDbUrl = @"http://localhost:8088";

  var httpClient = new HttpClient
  {
    BaseAddress = new Uri(ksqlDbUrl)
  };

  var httpClientFactory = new HttpClientFactory(httpClient);

  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  var metadata = new EntityCreationMetadata
  {
    KafkaTopic = "my_topic",
    ValueFormat = SerializationFormats.Json
  };

  var httpResponseMessage = await restApiClient.CreateStreamAsync<User>(metadata);
}
```

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

public record User
{
  [Key]
  public int Key { get; set; }

  public string Value { get; set; }
}
```

## Tables
A table in `ksqlDB` represents a **mutable** view of a stream. It is a continuously updated result set derived from one or more streams.
Tables are used for maintaining the current state and performing aggregations or joining operations on the data.
Tables have to define a required **key** that allows efficient retrieval of specific records based on the key value.
`ksqlDB` tables are usually stored in **compacted** Kafka topics that are a special type of topic in Kafka that retains only the most recent value for each key within the topic after compaction.

Retention policies determine how long or how much data is retained in a topic based on either **time** or **space** constraints. These policies are configured using the `cleanup.policy` and related properties.

The `retention.ms` configuration specifies the maximum amount of time that a message will be retained in a topic.

The `retention.bytes` configuration sets the maximum size of the log segments in a topic. 

Example:
```bash
kafka-topics --create --topic my_topic --bootstrap-server localhost:9092 --partitions 3 --replication-factor 3 --config cleanup.policy=delete
```

```SQL
CREATE TABLE messages (
  key INT,
  value STRING
) WITH (
  KAFKA_TOPIC = 'my_topic',
  VALUE_FORMAT = 'JSON'
);
```

The above `ksqlDB` statement can be executed from C# in a more type safe manner:
```C#
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

private static async Task CreateMessagesTableAsync()
{      
  var ksqlDbUrl = @"http://localhost:8088";

  var httpClient = new HttpClient
  {
    BaseAddress = new Uri(ksqlDbUrl)
  };

  var httpClientFactory = new HttpClientFactory(httpClient);

  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  var metadata = new EntityCreationMetadata
  {
    KafkaTopic = "my_topic",
    ValueFormat = SerializationFormats.Json
  };

  var httpResponseMessage = await restApiClient.CreateTableAsync<Message>(metadata);
}
```

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

public record Message
{
  [Key]
  public int Key { get; set; }

  public string Value { get; set; }
}
```

The **KEY** configuration specifies the column that will be used as the primary key for the table. In this example, the `'key'` column is designated as the key.

When you create a table in `ksqlDB`, it sets up the necessary infrastructure to consume and process events from the specified Kafka topic, similar to a stream. However, unlike a stream, a table maintains the **latest state** of the data based on the key column(s). The table continuously updates its state as new events arrive, allowing random access and lookup operations based on the key(s).

In the following example the underlying Kafka topic will be automatically configured as compacted:
```SQL
CREATE TABLE messages (
  key INT,
  value STRING
) WITH (
  PARTITIONS = 3,
  VALUE_FORMAT = 'JSON'
);
```

When creating a `ksqlDB` table without specifying the **KAFKA_TOPIC** configuration, you should provide the **PARTITIONS** configuration in the **WITH** clause to indicate the desired number of partitions for the underlying Kafka topic.
