# KSqlDbRestApiClient

## Basic auth
**v1.0.0**

In ksqldb you can use the [Http-Basic authentication](https://docs.ksqldb.io/en/latest/operate-and-deploy/installation/server-config/security/#configuring-listener-for-http-basic-authenticationauthorization) mechanism:
```C#
var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));
      
var restApiClient = new KSqlDbRestApiClient(httpClientFactory)
  .SetCredentials(new BasicAuthCredentials("fred", "letmein"));
```

### KSqlDbRestApiClient.InsertIntoAsync
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

### KSqlDbRestApiClient.InsertIntoAsync
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
var ksqlDbUrl = @"http:\\localhost:8088";

var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

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

In v1.0.0 support for inserting entities with primitive types and strings was added. This version adds support for `List<T>` and records, classes and structs. 
Deeply nested types and dictionaries are not yet supported.

```C#
var testEvent = new EventWithList
{
  Id = "1",
  Places = new List<int> { 1, 2, 3 }
};

var ksqlDbUrl = @"http:\\localhost:8088";

var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

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

[Insert values](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/insert-values/) - Produce a row into an existing stream or table and its underlying topic based on explicitly specified values.
```C#
string url = @"http:\\localhost:8088";

var http = new HttpClientFactory(new Uri(url));
var restApiClient = new KSqlDbRestApiClient(http);

var movie = new Movie() { Id = 1, Release_Year = 1988, Title = "Title" };

var response = await restApiClient.InsertIntoAsync(movie);
```

Properties and fields decorated with the IgnoreByInsertsAttribute are not part of the insert statements:
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

### ToInsertStatement
**v1.8.0**

- Generates raw string Insert Into, but does not execute it.

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

### AssertTopicExistsAsync and AssertTopicNotExistsAsync
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

### AssertSchemaExistsAsync and AssertSchemaNotExistsAsync
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

`GetConnectorsAsync` - List all connectors in the Connect cluster.

`DropConnectorAsync` - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement fails if the connector doesn't exist.
    
`DropConnectorIfExistsAsync` - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement doesn't fail if the connector doesn't exist.

```C#
using System;
using System.Linq;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public async Task CreateGetAndDropConnectorAsync()
{
  var ksqlDbUrl = @"http:\\localhost:8088";

  var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

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

### CreateTypeAsync
**v1.6.0**

- `IKSqlDbRestApiClient.CreateTypeAsync<TEntity>` - Create an alias for a complex type declaration.

```C#
using System;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.Sample.Models.Events;

private static async Task SubscriptionToAComplexTypeAsync()
{      
  var ksqlDbUrl = @"http:\\localhost:8088";

  var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));
  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"
Drop type {nameof(EventCategory)};
Drop table {nameof(Event)};
"));

  httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
  httpResponseMessage = await restApiClient.CreateTableAsync<Event>(new EntityCreationMetadata { KafkaTopic = "Events", Partitions = 1 });
      
  await using var ksqlDbContext = new KSqlDBContext(new KSqlDBContextOptions(ksqlDbUrl));

  var subscription = ksqlDbContext.CreateQueryStream<Event>()
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

### Droping types
**v1.0.0**

- DropTypeAsync and DropTypeIfExistsAsync - Removes a type alias from ksqlDB. If the IF EXISTS clause is present, the statement doesn't fail if the type doesn't exist.

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

### Drop a stream
**v1.0.0**

Drops an existing stream.

```C#
var ksqlDbUrl = @"http:\\localhost:8088";

var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));
var ksqlDbRestApiClient = new KSqlDbRestApiClient(httpClientFactory);

string streamName = "StreamName";

// DROP STREAM StreamName;
var httpResponseMessage = ksqlDbRestApiClient.DropStreamAsync(streamName);

// OR DROP STREAM IF EXISTS StreamName DELETE TOPIC;
httpResponseMessage = ksqlDbRestApiClient.DropStreamAsync(streamName, useIfExistsClause: true, deleteTopic: true);
```

Parameters:

`useIfExistsClause` - If the IF EXISTS clause is present, the statement doesn't fail if the stream doesn't exist.

`deleteTopic` - If the DELETE TOPIC clause is present, the stream's source topic is marked for deletion.

### PartitionBy
**v1.0.0**

[Repartition a stream.](https://docs.ksqldb.io/en/0.15.0-ksqldb/developer-guide/joins/partition-data/)

**TODO:** add example

### Terminate push queries
**v1.0.0**

- TerminatePushQueryAsync - terminates push query by query id

```C#
string queryId = "xyz123"; // <----- the ID of the query to terminate

var response = await restApiClient.TerminatePushQueryAsync(queryId);
```

### Drop a table
**v1.0.0**

Drops an existing table.

```C#
var ksqlDbUrl = @"http:\\localhost:8088";

var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));
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

- `CreateSourceConnectorAsync` - Create a new source connector in the Kafka Connect cluster with the configuration passed in the config parameter.

- `CreateSinkConnectorAsync` - Create a new sink connector in the Kafka Connect cluster with the configuration passed in the config parameter.

See also how to create a SQL Server source connector with [SqlServer.Connector](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/SqlServer.Connector/Wiki.md)

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

### Getting queries and termination of persistent queries
**v1.0.0**

- `GetQueriesAsync` - Lists queries running in the cluster.

- `TerminatePersistentQueryAsync` - Terminate a persistent query. Persistent queries run continuously until they are explicitly terminated.

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

### ExecuteStatementAsync
**v1.0.0**

[Execute a statement](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/ksql-endpoint/) - The /ksql resource runs a sequence of SQL statements. All statements, except those starting with SELECT, can be run on this endpoint. To run SELECT statements use the /query endpoint.

```C#
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

public async Task ExecuteStatementAsync()
{
  var ksqlDbUrl = @"http:\\localhost:8088";

  var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

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
KSqlDbStatement allows you to set the statement, content encoding and [CommandSequenceNumber](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/ksql-endpoint/#coordinate-multiple-requests). 

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
