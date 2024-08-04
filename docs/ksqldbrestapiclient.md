# KSqlDbRestApiClient
`ksqlDB` provides various **statements** to perform operations on streaming data. Here's a description of some commonly used `ksqlDB` statements:

- **CREATE STREAM**: By creating a stream with the provided columns and properties, a new stateless stream is established, and the stream is registered on a corresponding Apache Kafka® topic.

- **CREATE TABLE**: By creating a table with the provided columns and properties, a new table is established, and the table is registered on a corresponding Apache Kafka® topic. Similar to a stream, but table are stateful entities and maintain the latest value for each key.

- **CREATE STREAM AS SELECT**: Creates a new stream based on the result of a query. It creates a new stream with the specified name and schema, populating it with the results of the SELECT query.

- **DROP STREAM**: Deletes a stream and its associated data. It removes the stream definition and all the data associated with it.

## Basic auth
**v1.0.0**

In `ksqlDB` you can use the [Http-Basic authentication](https://docs.ksqldb.io/en/latest/operate-and-deploy/installation/server-config/security/#configuring-listener-for-http-basic-authenticationauthorization) mechanism:
```C#
var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);
      
var restApiClient = new KSqlDbRestApiClient(httpClientFactory)
  .SetCredentials(new BasicAuthCredentials("fred", "letmein"));
```

### InsertIntoAsync

The `InsertIntoAsync` method is a method used to insert data into a target stream or table in a `ksqlDB` cluster asynchronously. It allows you to send data records from your application to `ksqlDB` for further processing or storage.

- added support for deeply nested types - Maps, Structs and Arrays

```C#
var value = new ArrayOfMaps
{
  Arr = new[]
        {
          new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
          new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
        }
};

httpResponseMessage = await restApiClient.InsertIntoAsync(value);
```

```C#
record ArrayOfMaps
{
  public Dictionary<string, int>[] Arr { get; set; }
}
```

### InsertIntoAsync
**v1.0.0**

- added support for ```IEnumerable<T>``` properties

```C#
record Order
{
  public int Id { get; set; }
  public IEnumerable<double> Items { get; set; }
}
```

```C#
var ksqlDbUrl = @"http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);

var order = new Order { Id = 1, ItemsList = new List<double> { 1.1, 2 }};

var config = new InsertProperties
{
  ShouldPluralizeEntityName = false, 
  EntityName = "`my_order`"
};

var responseMessage = await new KSqlDbRestApiClient(httpClientFactory)
  .InsertIntoAsync(order, config);
```

Equivalent KSQL:
```SQL
INSERT INTO `my_order` (Id, ItemsList) VALUES (1, ARRAY[1.1,2]);
```

### InsertIntoAsync for complex types
**v1.6.0**

Support for inserting entities with primitive types and strings was introduced in version 1.0.0. However, the latest version expands on this by adding support for `List<T>` as well as records, classes, and structs. It's important to note that deeply nested types and dictionaries are not yet supported in this version (<=1.6.0).

```C#
var testEvent = new EventWithList
{
  Id = "1",
  Places = new List<int> { 1, 2, 3 }
};

var ksqlDbUrl = @"http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);

var responseMessage = await new KSqlDbRestApiClient(httpClientFactory)
  .InsertIntoAsync(testEvent);
```
Generated KSQL:
```SQL
INSERT INTO EventWithLists (Id, Places) VALUES ('1', ARRAY[1,2,3]);
```

```C#
var eventCategory = new EventCategory
{
  Count = 1,
  Name = "Planet Earth"
};

var testEvent2 = new ComplexEvent
{
  Id = 1,
  Category = eventCategory
};

var responseMessage = await new KSqlDbRestApiClient(httpClientFactory)
  .InsertIntoAsync(testEvent2, new InsertProperties { EntityName = "Events" });
```

Generated KSQL:
```SQL
INSERT INTO Events (Id, Category) VALUES (1, STRUCT(Count := 1, Name := 'Planet Earth'));
```

### Inserting empty arrays
**v1.0.0**

- empty arrays are generated in the following way (workaround)

```C#
var order = new Order { Id = 1, ItemsList = new List<double>()};

var responseMessage = await new KSqlDbRestApiClient(httpClientFactory)
  .InsertIntoAsync(order);
```

```SQL
ARRAY_REMOVE(ARRAY[0], 0))
```

```ARRAY[]``` is not yet supported in ksqldb (v0.21.0)

### Insert Into
**v1.0.0**

**INSERT INTO** statement is used to insert new rows of data into a stream or table.

[Insert values](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/insert-values/) - Produce a row into an existing stream or table and its underlying topic based on explicitly specified values.
```C#
string ksqlDbUrl = @"http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);
var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

var movie = new Movie() { Id = 1, Release_Year = 1988, Title = "Title" };

var response = await restApiClient.InsertIntoAsync(movie);
```

Properties and fields decorated with the `IgnoreByInsertsAttribute` are not part of the insert statements:
```C#
public class Movie
{
  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.Key]
  public int Id { get; set; }
  public string Title { get; set; }
  public int Release_Year { get; set; }
	
  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.IgnoreByInserts]
  public int IgnoredProperty { get; set; }
}
```

Generated KSQL:
```KSQL
INSERT INTO Movies (Title, Id, Release_Year) VALUES ('Title', 1, 1988);
```

### Insert values - FormatDoubleValue and FormatDecimalValue

```C#
var insertProperties = new InsertProperties()
{
  FormatDoubleValue = value => value.ToString("E1", CultureInfo.InvariantCulture),
  FormatDecimalValue = value => value.ToString(CultureInfo.CreateSpecificCulture("en-GB"))
};

public static readonly Tweet Tweet1 = new()
{
  Id = 1,
  Amount = 0.00042, 
  AccountBalance = 533333333421.6332M
};

await restApiProvider.InsertIntoAsync(tweet, insertProperties);
```

Generated KSQL statement:
```KSQL
INSERT INTO tweetsTest (Id, Amount, AccountBalance) VALUES (1, 4.2E-004, 533333333421.6332);
```

### ToInsertStatement
**v1.8.0**

Generates raw string Insert Into statement, but does not execute it.

```C#
Movie movie = new()
{
  Id = 1,
  Release_Year = 1986,
  Title = "Aliens"
};

var insertStatement = restApiProvider.ToInsertStatement(movie);

Console.WriteLine(insertStatement.Sql);
```

Output:

```SQL
INSERT INTO Movies (Title, Id, Release_Year) VALUES ('Aliens', 1, 1986);
```

### ExecuteStatementAsync extension method
**v1.0.0**

Executes arbitrary [statements](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/#streams-and-tables):
```C#
async Task<HttpResponseMessage> ExecuteAsync(string statement)
{
  KSqlDbStatement ksqlDbStatement = new(statement);

  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement)
    .ConfigureAwait(false);

  string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

  return httpResponseMessage;
}
```

### Substitute variables
**v2.6.0**

[Variable substitution](https://docs.ksqldb.io/en/latest/how-to-guides/substitute-variables/) allows you to supply different values in specific SQL statements:

```C#
var statement = new KSqlDbStatement("CREATE TYPE ${typeName} AS STRUCT<name VARCHAR, address ADDRESS>;")
{
  SessionVariables = new Dictionary<string, object> { { "typeName", typeName } }
};

var httpResponseMessage = await restApiClient.ExecuteStatementAsync(statement);
```

### Stream and table properties KEY_SCHEMA_ID and VALUE_SCHEMA_ID 
**v1.6.0** (ksqldb v0.24.0)

The **WITH** clause in the **CREATE STREAM** statement is used to specify additional configuration options for the creation of the stream, such as the serialization format, key format, number of partitions, replication factor, and various other settings specific to the stream.

The `EntityCreationMetadata` class in the `ksqlDB.RestApi.Client` library provides a convenient way to work with the metadata related to the creation of entities such as streams and tables in `ksqlDB`. 
Both streams and tables in `ksqlDB` are treated as **entities** that can be created, modified, and queried using the SQL-like language provided by `ksqlDB`. They have associated schemas, properties, and metadata that define their structure, behavior, and relationship with underlying Kafka topics.

**KEY_SCHEMA_ID** - The schema ID of the key schema in Schema Registry. The schema is used for schema inference and data serialization.

**VALUE_SCHEMA_ID** - The schema ID of the value schema in Schema Registry. The schema is used for schema inference and data serialization.

```C#
EntityCreationMetadata metadata = new(kafkaTopic: "tweets", partitions: 3)
{
  Replicas = 3,
  KeySchemaId = 1,
  ValueSchemaId = 2
};
```

Generated KSQL statement:

```
 WITH ( KAFKA_TOPIC='tweets', VALUE_FORMAT='Json', PARTITIONS='3', REPLICAS='3', KEY_SCHEMA_ID=1, VALUE_SCHEMA_ID=2 )
```

**Schema Registry** is a centralized service that provides a repository for storing and managing schemas for data **serialized** in Apache Kafka. It ensures data compatibility and consistency by enforcing schema evolution rules. When data is **produced** or **consumed** from Kafka topics, the Schema Registry is used to validate and ensure that the data adheres to the defined schema. It allows for schema evolution, versioning, and compatibility checks between producers and consumers.

`ksqlDB` can leverage the Schema Registry to handle the **serialization** and **deserialization** of data streams. When defining streams or tables in `ksqlDB`, you can specify the `Avro` or `Protobuf` schema associated with the data.
`ksqlDB` uses the Schema Registry to register and manage the schema information for the data streams. This integration ensures that the data being processed in `ksqlDB` is properly serialized and deserialized according to the schema defined in the Schema Registry.

### CreateSourceStreamAsync and CreateSourceTableAsync
**v1.4.0**

To enable the execution of pull queries on a **table**, you can include the **SOURCE** clause in the table's definition.

The **SOURCE** clause triggers an internal query for the table, which generates a materialized state that is utilized by pull queries. It's important to note that this internal query cannot be manually terminated. If you wish to end it, you can do so by using the DROP TABLE statement to remove the table `from ksqlDB`.

- `CreateSourceStreamAsync` - creates a read-only stream
- `CreateSourceTableAsync` - creates a read-only table

```C#
string entityName = nameof(IoTSensor);

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
var creationMetadata = new EntityCreationMetadata(kafkaTopic: "data_values")
{
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

When `UseInstanceType` is set to true, the insert statements will include the public fields and properties from the instance type.

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

### Assert topics
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

### Assert schemas
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

### Support explicit message types for Protobuf with multiple definitions
**v2.1.0**

- the following 2 new fields were added to `CreationMetadata`: `KeySchemaFullName` and `ValueSchemaFullName`

```C#
var creationMetadata = new CreationMetadata
{
  KeySchemaFullName = "ProductKey"
  ValueSchemaFullName = "ProductInfo"
};
```

### Connectors
**v1.0.0**

**Connectors** are used to integrate external data **sources** and **sinks** with the `ksqlDB` engine.
Connectors enable seamless ingestion and egress of data between `ksqlDB` and various external systems.
They allow you to connect `ksqlDB` to different data platforms, messaging systems, databases, or custom sources and sinks.

`GetConnectorsAsync` - List all connectors in the Connect cluster.

`DropConnectorAsync` - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement fails if the connector doesn't exist.
    
`DropConnectorIfExistsAsync` - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement doesn't fail if the connector doesn't exist.

```C#
using System;
using System.Linq;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public async Task CreateGetAndDropConnectorAsync()
{
  var ksqlDbUrl = @"http://localhost:8088";

  var httpClient = new HttpClient
  {
    BaseAddress = new Uri(ksqlDbUrl)
  };

  var httpClientFactory = new HttpClientFactory(httpClient);

  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  const string SinkConnectorName = "mock-connector";

  var createConnector = @$"CREATE SOURCE CONNECTOR `{SinkConnectorName}` WITH(
      'connector.class'='org.apache.kafka.connect.tools.MockSourceConnector');";

  var statement = new KSqlDbStatement(createConnector);

  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(statement);

  var connectorsResponse = await restApiClient.GetConnectorsAsync();

  Console.WriteLine("Available connectors: ");
  Console.WriteLine(string.Join(',', connectorsResponse[0].Connectors.Select(c => c.Name)));

  httpResponseMessage = await restApiClient.DropConnectorAsync($"`{SinkConnectorName}`");

  // Or
  httpResponseMessage = await restApiClient.DropConnectorIfExistsAsync($"`{SinkConnectorName}`");
}
```

```SQL
SHOW CONNECTORS;

CREATE SOURCE CONNECTOR `mock-connector` WITH(
      'connector.class'='org.apache.kafka.connect.tools.MockSourceConnector');

DROP CONNECTOR `mock-connector`;
```

### Create types
**v1.6.0**

In `ksqlDB`, you can create **custom types** using the `CREATE TYPE` statement.
This allows you to define structured data types that can be used in the schema of streams and tables.

- `IKSqlDbRestApiClient.CreateTypeAsync<TEntity>` - Create an alias for a complex type declaration.

```C#
using System;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.Sample.Models.Events;

private static async Task SubscriptionToAComplexTypeAsync()
{      
  var ksqlDbUrl = @"http://localhost:8088";

  var httpClient = new HttpClient
  {
    BaseAddress = new Uri(ksqlDbUrl)
  };

  var httpClientFactory = new HttpClientFactory(httpClient);
  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"
Drop type {nameof(EventCategory)};
Drop table {nameof(Event)};
"));

  httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
  var metadata = new EntityCreationMetadata(kafkaTopic: "Events") { Partitions = 1 };
  httpResponseMessage = await restApiClient.CreateTableAsync<Event>(metadata);
      
  await using var ksqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(ksqlDbUrl));

  var subscription = ksqlDbContext.CreatePushQuery<Event>()
    .Take(1)
    .Subscribe(value =>
    {
      Console.WriteLine("Categories: ");

      foreach (var category in value.Categories)
      {
        Console.WriteLine($"{category.Name}");
      }
    }, error =>
    {
      Console.WriteLine(error.Message);
    });

  httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
INSERT INTO Events (Id, Places, Categories) VALUES (1, ARRAY['1','2','3'], ARRAY[STRUCT(Name := 'Planet Earth'), STRUCT(Name := 'Discovery')]);"));
}
```

```C#
using System.Collections.Generic;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

record EventCategory
{
  public string Name { get; set; }
}

record Event
{
  [Key]
  public int Id { get; set; }

  public string[] Places { get; set; }

  public IEnumerable<EventCategory> Categories { get; set; }
}
```

```SQL
CREATE TYPE EVENTCATEGORY AS STRUCT<Name VARCHAR>;
```

In this example, we create a custom type named `EVENTCATEGORY` with 1 field: `Name` specified with the **VARCHAR** data type, but you can use other supported data types in `ksqlDB`, such as **INT**, **BOOLEAN**, **DOUBLE**, **ARRAY**, or even other custom types.

### Droping types
**v1.0.0**

- `DropTypeAsync` and `DropTypeIfExistsAsync` - Removes a type alias from ksqlDB. If the IF EXISTS clause is present, the statement doesn't fail if the type doesn't exist.

```C#
string typeName = nameof(EventCategory);
var httpResponseMessage = await restApiClient.DropTypeAsync(typeName);
//OR
httpResponseMessage = await restApiClient.DropTypeIfExistsAsync(typeName);
```

```SQL
DROP TYPE EventCategory;

DROP TYPE IF EXISTS EventCategory;
```

With the `DropTypeAsync` overload, the type name can be automatically inferred from the generic type argument.

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

var properties = new DropTypeProperties
{
  ShouldPluralizeEntityName = false,
  IdentifierEscaping = IdentifierEscaping.Always
};

var response = await restApiClient.DropTypeAsync<EventCategory>(properties);
```

```SQL
DROP TYPE `EventCategory`;
```

### Drop a stream
**v1.0.0**

Drops an existing stream.

```C#
var ksqlDbUrl = @"http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);
var ksqlDbRestApiClient = new KSqlDbRestApiClient(httpClientFactory);

string streamName = "StreamName";

// DROP STREAM StreamName;
var httpResponseMessage = ksqlDbRestApiClient.DropStreamAsync(streamName);

// OR DROP STREAM IF EXISTS StreamName DELETE TOPIC;
httpResponseMessage = ksqlDbRestApiClient.DropStreamAsync(streamName, useIfExistsClause: true, deleteTopic: true);
```

```SQL
DROP STREAM StreamName;

DROP STREAM IF EXISTS StreamName DELETE TOPIC;
```

Parameters:

`useIfExistsClause` - If the IF EXISTS clause is present, the statement doesn't fail if the stream doesn't exist.

`deleteTopic` - If the DELETE TOPIC clause is present, the stream's source topic is marked for deletion.

#### DropEntityProperties

The `DropFromItemProperties` class is used to configure dropping entitities, such as streams or tables in ksqlDB.
In the provided example, it's instantiated with specific properties: using "IF EXISTS" clause, deleting the associated topic,
not pluralizing the entity name, and always escaping identifiers. The `from-item` name is inferred from the generic type argument.
This configuration is then used to drop a table named `TestTable` and a stream named `TestStream` via the ksqlDB REST API client.

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

class TestTable;
class TestStream;

var properties = new DropFromItemProperties
{
  UseIfExistsClause = true,
  DeleteTopic = true,
  ShouldPluralizeEntityName = false,
  IdentifierEscaping = IdentifierEscaping.Always
};

var response1 = await ksqlDbRestApiClient.DropTableAsync<TestTable>(properties);
var response2 = await ksqlDbRestApiClient.DropStreamAsync<TestStream>(properties);
```

The resulting KSQL commands executed are: 
```SQL
DROP TABLE IF EXISTS `TestTable` DELETE TOPIC;
DROP STREAM IF EXISTS `TestStream` DELETE TOPIC;
```

### PartitionBy
**v1.0.0**

The **PARTITION BY** clause is used in stream queries to specify the column or expression by which the resulting stream should be partitioned. It determines how the data within the stream is distributed across different partitions.

[Repartition a stream.](https://docs.ksqldb.io/en/0.15.0-ksqldb/developer-guide/joins/partition-data/)

```C#
var httpResponseMessage = await context.CreateOrReplaceTableStatement(tableName: "MoviesByTitle")
  .With(creationMetadata)
  .As<Movie>()
  .Where(c => c.Id < 3)
  .Select(c => new { c.Title, ReleaseYear = c.Release_Year })
  .PartitionBy(c => c.Title)
  .ExecuteStatementAsync();
```

```SQL
CREATE OR REPLACE TABLE MoviesByTitle
  WITH ( KAFKA_TOPIC='moviesByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' )
    AS SELECT Title, Release_Year AS ReleaseYear
       FROM Movies
       WHERE Id < 3
       PARTITION BY Title
       EMIT CHANGES;
```

### Pause and resume persistent queries
**v2.5.0**

In `ksqlDB`, the ability to **pause** and **resume** persistent queries is used to control the processing of data and the execution of continuous queries within the `ksqlDB` engine. Pausing and resuming queries provide a way to temporarily halt query processing or reactivate them as needed.

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

```SQL
PAUSE xyz123;
RESUME xyz123;
```

### Terminate push queries
**v1.0.0**

- TerminatePushQueryAsync - terminates a push query by query ID

```C#
string queryId = "xyz123"; // <----- the ID of the query to terminate

var response = await restApiClient.TerminatePushQueryAsync(queryId);
```

```SQL
TERMINATE xyz123;
```

### Drop a table
**v1.0.0**

Drops an existing table.

```C#
var ksqlDbUrl = @"http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);

var ksqlDbRestApiClient = new KSqlDbRestApiClient(httpClientFactory);

string tableName = "TableName";

// DROP TABLE TableName;
var httpResponseMessage = ksqlDbRestApiClient.DropTableAsync(tableName);

// OR DROP TABLE IF EXISTS TableName DELETE TOPIC;
httpResponseMessage = ksqlDbRestApiClient.DropTableAsync(tableName, useIfExistsClause: true, deleteTopic: true);
```

Parameters:

`useIfExistsClause` - If the IF EXISTS clause is present, the statement doesn't fail if the table doesn't exist.

`deleteTopic` - If the DELETE TOPIC clause is present, the table's source topic is marked for deletion.

### Creating connectors
**v1.0.0**

A **connector** is a pre-built component that acts as a bridge between Kafka and an external system.
There are 2 types of connectors:
- **source** connectors allow you to ingest data from external systems into Kafka topics
- **sink** connectors enable you to stream data from Kafka topics to external systems

--- 
- `CreateSourceConnectorAsync` - Create a new source connector in the Kafka Connect cluster with the configuration passed in the config parameter.

- `CreateSinkConnectorAsync` - Create a new sink connector in the Kafka Connect cluster with the configuration passed in the config parameter.

See also how to create a SQL Server source connector with [SqlServer.Connector](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/SqlServer.Connector/README.md)

```C#
using System.Collections.Generic;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi;

private static string SourceConnectorName => "mock-source-connector";
private static string SinkConnectorName => "mock-sink-connector";

private static async Task CreateConnectorsAsync(IKSqlDbRestApiClient restApiClient)
{
  var sourceConnectorConfig = new Dictionary<string, string>
  {
    {"connector.class", "org.apache.kafka.connect.tools.MockSourceConnector"}
  };

  var httpResponseMessage = await restApiClient.CreateSourceConnectorAsync(sourceConnectorConfig, SourceConnectorName);
      
  var sinkConnectorConfig = new Dictionary<string, string> {
    { "connector.class", "org.apache.kafka.connect.tools.MockSinkConnector" },
    { "topics.regex", "mock-sink*"},
  }; 		

  httpResponseMessage = await restApiClient.CreateSinkConnectorAsync(sinkConnectorConfig, SinkConnectorName);

  httpResponseMessage = await restApiClient.DropConnectorAsync($"`{SinkConnectorName}`");
}
```

### Get topics
**v1.0.0**

In **Apache Kafka**, a **topic** is a durable and distributed data storage mechanism, to which messages are **published**.
It represents a stream of records, where each record consists of a **key**, a **value**, and a **timestamp**.

In `ksqlDB`, a Kafka topic represents a stream of events or records that are processed and analyzed using the `ksqlDB` engine.

- `GetTopicsAsync` - lists the available topics in the Kafka cluster that ksqlDB is configured to connect to.
- `GetAllTopicsAsync` - lists all topics, including hidden topics.
- `GetTopicsExtendedAsync` - list of topics. Also displays consumer groups and their active consumer counts.
- `GetAllTopicsExtendedAsync` - list of all topics. Also displays consumer groups and their active consumer counts.

```C#
using System;
using System.Linq;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using ksqlDB.RestApi.Client.Sample.Providers;

private static async Task GetKsqlDbInformationAsync(IKSqlDbRestApiProvider restApiProvider)
{
  Console.WriteLine($"{Environment.NewLine}Available topics:");
  var topicsResponses = await restApiProvider.GetTopicsAsync();
  Console.WriteLine(string.Join(',', topicsResponses[0].Topics.Select(c => c.Name)));

  TopicsResponse[] allTopicsResponses = await restApiProvider.GetAllTopicsAsync();
  TopicsExtendedResponse[] topicsExtendedResponses = await restApiProvider.GetTopicsExtendedAsync();
  var allTopicsExtendedResponses = await restApiProvider.GetAllTopicsExtendedAsync();
}
```

```SQL
SHOW TOPICS;
SHOW ALL TOPICS;
SHOW TOPICS EXTENDED;
SHOW ALL TOPICS EXTENDED;
```

### Getting queries and termination of persistent queries
**v1.0.0**

- `GetQueriesAsync` - Lists queries running in the cluster.

- `TerminatePersistentQueryAsync` - **Terminate** a persistent query. Persistent queries run continuously until they are explicitly terminated.

```C#
using System.Linq;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi;

private static async Task TerminatePersistentQueryAsync(IKSqlDbRestApiClient client)
{
  string topicName = "moviesByTitle";

  var queries = await client.GetQueriesAsync();

  var query = queries.SelectMany(c => c.Queries).FirstOrDefault(c => c.SinkKafkaTopics.Contains(topicName));

  var response = await client.TerminatePersistentQueryAsync(query.Id);
}
```

```SQL
SHOW QUERIES;
```

### ExecuteStatementAsync
**v1.0.0**

[Execute a statement](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/ksql-endpoint/) - The /ksql resource runs a sequence of SQL statements.
All statements, except those starting with SELECT, can be run on this endpoint. To run SELECT statements use the /query or /query-stream endpoint.

```C#
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public async Task ExecuteStatementAsync()
{
  var ksqlDbUrl = @"http://localhost:8088";

  var httpClient = new HttpClient
  {
    BaseAddress = new Uri(ksqlDbUrl)
  };

  var httpClientFactory = new HttpClientFactory(httpClient);

  IKSqlDbRestApiClient restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  var statement = $@"CREATE OR REPLACE TABLE {nameof(Movies)} (
        title VARCHAR PRIMARY KEY,
        id INT,
        release_year INT
      ) WITH (
        KAFKA_TOPIC='{nameof(Movies)}',
        PARTITIONS=1,
        VALUE_FORMAT = 'JSON'
      );";

  KSqlDbStatement ksqlDbStatement = new(statement);
  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement);

  string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
}

public record Movies
{
  public int Id { get; set; }

  public string Title { get; set; }

  public int Release_Year { get; set; }
}
```

### KSqlDbStatement
With `KSqlDbStatement`, you have the ability to define the KSQL statement itself, specify the content encoding, and set the [CommandSequenceNumber](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/ksql-endpoint/#coordinate-multiple-requests).

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public KSqlDbStatement CreateStatement(string statement)
{
  KSqlDbStatement ksqlDbStatement = new(statement) {
    ContentEncoding = Encoding.Unicode,
    CommandSequenceNumber = 10,
    [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
  };
	
  return ksqlDbStatement;
}
```

### HttpResponseMessage ToStatementResponses extension

The `HttpResponseMessage` extension `ToStatementResponses` is used to transform a HTTP response received from a ksqlDB REST API call into a collection of statement responses.
These statement responses contain information about the execution status.

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;

var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement);

var responses = httpResponseMessage.ToStatementResponses();

foreach (var response in responses)
{
	Console.WriteLine(response.CommandStatus);
	Console.WriteLine(response.CommandId);
}
```

### Create or replace table statements
**v1.0.0**

| Statement                                                                                                             | Description                                                                                                                                                                                     |
|-----------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [EXECUTE STATEMENTS](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/)                              | CreateStatementAsync - execution of general-purpose string statements                                                                                                                           |
| [CREATE STREAM](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-stream/)                     | CreateStreamAsync - Create a new stream with the specified columns and properties.                                                                                                              |
| [CREATE TABLE](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-table/)                       | CreateTableAsync - Create a new table with the specified columns and properties.                                                                                                                |
| [CREATE STREAM AS SELECT](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-stream-as-select/) | CreateOrReplaceStreamStatement - Create or replace a new materialized stream view, along with the corresponding Kafka topic, and stream the result of the query into the topic.                 |
| [CREATE TABLE AS SELECT](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-table-as-select/)   | CreateOrReplaceTableStatement - Create or replace a ksqlDB materialized table view, along with the corresponding Kafka topic, and stream the result of the query as a changelog into the topic. |

```C#
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;

public static async Task Main(string[] args)
{
  await using var context = new KSqlDBContext(@"http://localhost:8088");
  await CreateOrReplaceTableStatement(context);
}

private static async Task CreateOrReplaceTableStatement(IKSqlDBStatementsContext context)
{
  var creationMetadata = new CreationMetadata
  {
    KafkaTopic = "moviesByTitle",		
    KeyFormat = SerializationFormats.Json,
    ValueFormat = SerializationFormats.Json,
    Replicas = 1,
    Partitions = 1
  };

  var httpResponseMessage = await context.CreateOrReplaceTableStatement(tableName: "MoviesByTitle")
    .With(creationMetadata)
    .As<Movie>()
    .Where(c => c.Id < 3)
    .Select(c => new {c.Title, ReleaseYear = c.Release_Year})
    .PartitionBy(c => c.Title)
    .ExecuteStatementAsync();

  var statementResponse = httpResponseMessage.ToStatementResponses();
}
```

Generated KSQL statement:
```KSQL
CREATE OR REPLACE TABLE MoviesByTitle
  WITH ( KAFKA_TOPIC='moviesByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' )
    AS
SELECT Title, Release_Year AS ReleaseYear
  FROM Movies
 WHERE Id < 3 PARTITION BY Title
  EMIT CHANGES;
```

### Creating streams and tables
**v1.0.0**

- [CREATE STREAM](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-stream/) - fluent API

```C#
EntityCreationMetadata metadata = new(kafkaTopic: nameof(MyMovies))
{
  Partitions = 1,
  Replicas = 1
};

string ksqlDbUrl = @"http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);
var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

var httpResponseMessage = await restApiClient.CreateStreamAsync<MyMovies>(metadata, ifNotExists: true);
```

```C#
public record MyMovies
{
  [ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations.Key]
  public int Id { get; set; }

  public string Title { get; set; }

  public int Release_Year { get; set; }
}
```

```KSQL
CREATE STREAM IF NOT EXISTS MyMovies (
	Id INT KEY,
	Title VARCHAR,
	Release_Year INT
) WITH ( KAFKA_TOPIC='MyMovies', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );
```

Create or replace alternative:

```C#
var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<MyMovies>(metadata);
```
 
- [CREATE TABLE](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-table/) - fluent API

```C#
EntityCreationMetadata metadata = new(kafkaTopic: nameof(MyMovies))
{
  Partitions = 2,
  Replicas = 3
};

string ksqlDbUrl = @"http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);
var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

var httpResponseMessage = await restApiClient.CreateTableAsync<MyMovies>(metadata, ifNotExists: true);
```

```KSQL
CREATE TABLE IF NOT EXISTS MyMovies (
	Id INT PRIMARY KEY,
	Title VARCHAR,
	Release_Year INT
) WITH ( KAFKA_TOPIC='MyMovies', VALUE_FORMAT='Json', PARTITIONS='2', REPLICAS='3' );
```

### Get streams
**v1.0.0**

To get a list of **streams** defined in `ksqlDB`, you can use the `SHOW STREAMS` statement.

- `IKSqlDbRestApiClient.GetStreamsAsync` - List the defined streams.

```SQL
SHOW STREAMS;
```

```C#
var streamResponses = await restApiClient.GetStreamsAsync();

Console.WriteLine(string.Join(',', streamResponses[0].Streams.Select(c => c.Name)));
```

The result of executing this statement will be an array showing the names and details of the streams available in the `ksqlDB` server.

### Get tables
**v1.0.0**

To get a list of **tables** defined in `ksqlDB`, you can use the `SHOW TABLES` statement.

- `IKSqlDbRestApiClient.GetTablesAsync` - List the defined tables.

```SQL
SHOW TABLES;
```

```C#
var tableResponses = await restApiClient.GetTablesAsync();

Console.WriteLine(string.Join(',', tableResponses[0].Tables.Select(c => c.Name)));
```

The result of executing this statement will be an array showing the names and details of the tables available in the `ksqlDB` server.

### Insert values with KSQL functions
**v2.7.0**

```C#
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Annotations;

[KSqlFunction]
public static string INITCAP(string value) => throw new NotSupportedException();
```

```C#
Expression<Func<string>> valueExpression = () => INITCAP("One little mouse");

var insertValues = new InsertValues<Movie>(new Movie { Id = 5 });

insertValues.WithValue(c => c.Title, valueExpression);

Context.Add(insertValues);

var response = await Context.SaveChangesAsync();
```

```SQL
INSERT INTO Movies (Title, Id, Release_Year) VALUES (INITCAP('One little mouse'), 5, 0);
```

### Default ShouldPluralizeFromItemName setting for KSqlDbRestApiClient
**v6.2.0**

Here's the improved version of the text:

The `KSqlDbRestApiClient` class now includes `KSqlDBRestApiClientOptions` in its constructor arguments.
Additionally, `EntityCreationMetadata.ShouldPluralizeEntityName` has been changed to a nullable boolean, and its default value of `true` has been removed.
The methods in `KSqlDbRestApiClient` check if the `ShouldPluralizeEntityName` field in the `TypeProperties`, `DropTypeProperties`, `InsertProperties`, and `DropFromItemProperties` classes is null, and if so, set it using the value from `KSqlDBRestApiClientOptions`.

```C#
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;

var ksqlDbUrl = "http://localhost:8088";

var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};
var httpClientFactory = new HttpClientFactory(httpClient);
var restApiClientOptions = new KSqlDBRestApiClientOptions
{
  ShouldPluralizeFromItemName = true,
};

var restApiClient = new KSqlDbRestApiClient(httpClientFactory, restApiClientOptions);
```

To use dependency injection (DI), first create and configure an instance of `KSqlDBRestApiClientOptions`.
Then, register this configured instance with the service collection.

```C#
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Microsoft.Extensions.DependencyInjection;

var servicesCollection = new ServiceCollection();

servicesCollection.AddDbContext<IKSqlDBContext, KSqlDBContext>(
  options =>
  {
    var ksqlDbUrl = "http://localhost:8088";
    var setupParameters = options.UseKSqlDb(ksqlDbUrl);

    setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);

  }, contextLifetime: ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);

var restApiClientOptions = new KSqlDBRestApiClientOptions
{
  ShouldPluralizeFromItemName = false,
};
servicesCollection.AddSingleton(restApiClientOptions);
```
