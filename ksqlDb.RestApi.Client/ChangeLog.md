# ksqlDB.RestApi.Client

# 3.7.0
- introduced new overloads for dropping entities: `DropTableAsync`, `DropTypeAsync`, and `DropStreamAsync` in `KSqlDbRestApiClient`. These overloads now accept an argument `DropFromItemProperties`, providing more flexibility in configuring the drop operations.
- extracted `IKSqlDbDropRestApiClient` interface from `IKSqlDbRestApiClient`

# 3.6.2
- JsonPropertyName and PseudoColumn attributes are taken into account when setting column names in LINQ column selection syntax #61 (contributed by @mrt181)

# 3.6.1
- fix usage of `JsonPropertyName` when creating insert statements (since 3.6.0) #59 (contributed by @mrt181)

# 3.6.0
- added escaping options using backticks for [Identifiers](https://docs.ksqldb.io/en/latest/reference/sql/syntax/lexical-structure/#identifiers) in statements #57 (contributed by @mrt181)
- added `TypeProperties` class to configure type creation #58 (contributed by @mrt181)
- added .NET Enums to VARCHAR mapping #55. `JsonStringEnumConverter` was added to `KSqlDbJsonSerializerOptions`.

# 3.5.0
- added [Explode](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/table-functions/#explode) table function

# 3.4.0
- added `CommandStatus` class for introspecting [query status](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/status-endpoint/)
- fixed usage of `Equals` in queries (Predicate with .Equals fails. #50)

# 3.3.0
- added **net8.0** TFM

# 3.2.2
- fixed exception in ExtractFieldValue when using generics #48

# 3.2.1
- fixed SQL LIKE statement generation from expressions starting with a constant

# 3.2.0
- added `Headers`, `RowOffset` and `RowPartition` pseudocolumns to `Record` type
- entity creation added WITH RETENTION_MS property

# 3.1.0
- added `ProcessingGuarantee` enum value **ExactlyOnceV2**

# 3.0.1
- GUID in Contains/IN #45

# v3.0.0
- property `AsyncDisposableObject.IsDisposed` was changed from public to an internal access modifier
- upgraded .NET package dependencies

Removed not supported **TFM**s:
- netcoreapp3.1;net5.0

Removed obsolete methods:
- `KSqlFunctionsExtensions.Sign`
- `IAggregations<TSource>.CollectList`

# v2.7.0
- insert values with KSQL functions
- JsonArrayParser fixed deserialization of values with brackets #43 fix

# v2.6.0
- added shared values into KSqlDbStatement #39
- KSqlDbContext's CreateStream and CreateStreamQuery influence each others services collection during consecutive usages. #37

# v2.5.2
- RowValueJsonSerializer single field instance deserialization fix for arrays #35

# v2.5.1
- RowValueJsonSerializer single field instance deserialization fix #34

# v2.5.0
KSqlDbRestApiClient:
- PausePushQueryAsync - pauses a persistent query by query id
- ResumePushQueryAsync - resume a paused persistent query. Transient queries cannot be paused or resumed.

TimeWindows:
- EMIT FINAL output refinement was added for windowed aggregations

# v2.4.0
- added System.GUID as VARCHAR ksqldb type #32
- extract field names and values (for insert and select statements) #33
- generate insert statements from entity types (covers use cases when an interface is used as the type). Added InsertProperties.UseInstanceType configuration. #33

# v2.3.2
- #31 The "CreateJson" function in the "JsonArrayParser" class does not work correctly. bug fix

# v2.3.1
- adds support for Protobuf content type via the ksqlDb.RestApi.Client.Protobuf package

# v2.3.0
- added TFM for .NET 6.0

### Assert topic
- asserts that a topic exists or does not exist.
- IKSqlDbRestApiClient.AssertTopicExistsAsync, IKSqlDbRestApiClient.AssertTopicNotExistsAsync, AssertTopicResponse, AssertTopicOptions

### Assert schema
- Asserts that a schema exists or does not exist.
- IKSqlDbRestApiClient.AssertSchemaExistsAsync, IKSqlDbRestApiClient.AssertSchemaNotExistsAsync, AssertSchemaResponse, AssertSchemaOptions
- IKSqlDbAssertionsRestApiClient

### Serialization formats
- added `Protobuf_NoSR` serialization format

- #27 added support to use `JsonPropertyNameAttribute` for anonymous objects in selects for queries

# v2.2.1
- #27 added support to use `JsonPropertyNameAttribute` for selects in queries

# v2.2.0
- #27 Rename stream or table column names with the `JsonPropertyNameAttribute`
- #28 Source extensions were made public - bug fix
- #28 join on KSqlFunctions without aliases - bug fix

# v2.1.4
- added `IKSqlDbRestApiClient` lifetime configuration for registration with `IServiceCollection.AddDbContext`

# v2.1.3
- #25 ConsumeAsync is not responding on CancellationToken (while waiting on ReadLineAsync or EndOfStream)

# v2.1.2
- was unlisted - contains a breaking change
-
# v2.1.1
- `KSqlDBContext` - added support for IDisposable

# v2.1.0
### Aggregation functions
- MIN and MAX aggregates support for DATE, TIME, and TIMESTAMP types

# CreationMetadata
Support explicit message types for `Protobuf` with multiple definitions:
- added KeySchemaFullName and ValueSchemaFullName

# Right Join
- select all records for the right side of the join and the matching records from the left side. If the matching records on the left side are missing, the corresponding columns will contain null values.

# v2.0.1
- fixed missing IHttpClientFactory registration for NETSTANDARD

# v2.0.0

**Breaking changes:**

### DisposeHttpClient
`KSqlDBContextOptions` and `KSqlDbRestApiClient` - `DisposeHttpClient` property is by default set to `false`.

### HttpClientFactory
constructor argument was changed from `Uri` to `HttpClient`. The `IHttpClientFactory` is registered with `System.Net.Http.AddHttpClient` for better lifecycle management

### Package references
- upgraded package references `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging.Abstractions` to v6.0.0
- added package reference `Microsoft.Extensions.Http` v6.0.0

Added:
- added IHttpV1ClientFactory
- KSqlDbContextOptionsBuilder.ReplaceHttpClient

- aggregate function COLLECT_LIST, COLLECT_SET, EARLIEST_BY_OFFSET, LATEST_BY_OFFSET - with Structs, Arrays, and Maps

- KSqlDbProviderOptions.DisposeHttpClient option
- KSqlDbRestApiProvider.DisposeHttpClient option

Scalar functions:
- FormatDate, FormatTime, ParseDate, ParseTime, InitCap

# v1.6.0
- Pull query Take extension method (Limit)
- Stream and table properties KEY_SCHEMA_ID and VALUE_SCHEMA_ID
- Access record header data

Scalar functions:
- IsJsonString, JsonArrayLength, JsonConcat, JsonKeys, JsonRecords, ToJsonString

# v1.5.0
- improved invocation function extensions

- TimeSpanToStringConverter
- added support for Time types DATE, TIME AND TIMESTAMP (ksqldb 0.20.0)
- operator (Not) Between for Time type values

# v1.4.0
## IKSqlDBContextFactory
A factory for creating derived KSqlDBContext instances.

## KSqlDbServiceCollectionExtensions.AddDbContext
Registers the given ksqldb context as a service in the IServiceCollection

## KSqlDbServiceCollectionExtensions.AddDbContextFactory
Registers the given ksqldb context factory as a service in the IServiceCollection

## KSqlDbRestApiClient
- CreateTypeAsync - added overload without type name argument
- CreateSourceStreamAsync - creates a read-only stream
- CreateSourceTableAsync - creates a read-only table

## EntityCreationMetadata
- IncludeReadOnlyProperties - Include read-only properties during entity generation.

## KSqlDbContextOptionsBuilder and KSqlDbContextOptions
- SetJsonSerializerOptions - a way to configure the JsonSerializerOptions for the materialization of the incoming values.

Bug fix:
- SubscribeAsync - error propagation fix
- CreateTypeAsync - applied type name fix

# v1.3.1
### InsertProperties IncludeReadOnlyProperties
- #12 include readonly properties in Inserts config

### KSqlDBContext.SaveChangesAsync
- added CancellationToken argument

# v1.3.0
## Join Within
- specify a time window for stream-stream joins

## KSqlDbRestApiClient
- CreateTypeAsync added optional type name argument

## Operator LIKE - String.StartsWith, String.EndsWith, String.Contains
Match a string with a specified pattern

## IKSqlDBContext Add and SaveChangesAsync
Saving multiple entities with one request

## Fixes:
- KSqlDbContext services collection - injected ILoggerFactory instance is registered as Singleton

# v1.2.0
## KSqlDbServiceCollectionExtensions.ConfigureKSqlDb
- registers the following dependencies: IKSqlDBContext, KSqlDbRestApiClient, IHttpClientFactory, KSqlDBContextOptions

## Logging
- LogInformation about received data, executed commands and queries
- added package reference - Microsoft.Extensions.Logging.Abstractions

# v1.1.0
- multiple joins with query comprehension syntax (GroupJoin, SelectMany, DefaultIfEmpty)

# v1.0.0
Package had to be renamed to ksqlDB.RestApi.Client

Breaking changes: namespaces were changed accordingly

# Kafka.DotNet.ksqlDB

# v2.0.0
## ProcessingGuarantee enum
- KSqlDbContextOptionsBuilder and KSqlDbContextOption SetProcessingGuarantee

- ProcessingGuaranteeExtensions, AutoOffsetResetExtensions

## Basic authentication
- added IKSqlDbProvider.SetCredentials
- added IKSqlDbRestApiClient.SetCredentials
- BasicAuthCredentials
- KSqlDbContextOptionsBuilder and KSqlDbContextOption SetBasicAuthCredentials
- BasicAuthHandler, HttpClientFactoryWithBasicAuth

## KSqlDbRestApiClient.InsertIntoAsync
- added support for deeply nested types - Maps, Structs and Arrays

## Qbservable.Select
- generation of values from captured variables

## KSqlDBContextOptions.NumberFormatInfo
- formats double values in Selects

## Breaking changes:

## KSqlDBContextOptions
> ⚠ KSqlDBContextOptions created with a constructor or by KSqlDbContextOptionsBuilder are setting the auto.offset.reset to earliest by default. This version removes this default configuration. It will not be opinionated in this way from now.
> This will affect your subscriptions to streams.

### ISetupParameters changed from
```
ISetupParameters SetupQuery(Action<IQueryOptions> configure);
ISetupParameters SetupQueryStream(Action<IQueryOptions> configure);
```
to
```
ISetupParameters SetupQuery(Action<IKSqlDbParameters> configure);
ISetupParameters SetupQueryStream(Action<IKSqlDbParameters> configure);
```

## IPullable
```
public ValueTask<TEntity> GetAsync(CancellationToken cancellationToken = default)
```
was renamed to:
```
public ValueTask<TEntity> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
```

### Bug fix:
- deserialization of stream exceptions (KSqlDbQueryProvider and KSqlDbQueryStreamProvider)

# v1.10.0
## Invocation (lambda) functions
- **Transform**, **Reduce** and **Filter** for Maps (dictionaries)
- requirements: ksqldb 0.17.0

## Select and Where destructuring properties

## Scalar functions
- Instr, IfNull

## IKSqlGrouping.Source
- grouping by nested properies

Bug fixes:
- KSqlDbRestApiClient.CreateTypeAsync - Entity name from generic types fix
- KsqlDbContext.CreateQueryStream - From item name - generic types fix

# v1.9.0
## Invocation (lambda) functions
- requirements: ksqldb 0.17.0
- This version covers ARRAY type. MAP types are not included in this release.

### Transform
- Transform a collection by using a lambda function.

### Reduce
- Reduce a collection starting from an initial state.

### Filter
- Filter a collection with a lambda function.

## BYTES
- [BYTES TYPE](https://docs.ksqldb.io/en/latest/reference/sql/data-types/#character-types) - variable-length byte array (byte[])
- requirements: ksqldb 0.21.0

## ToBytes
- Converts a STRING value in the specified encoding to BYTES. The accepted encoders are 'hex', 'utf8', 'ascii' and 'base64'. Since: - ksqldb 0.21

## FromBytes
- Converts a BYTES value to STRING in the specified encoding. The accepted encoders are 'hex', 'utf8', 'ascii' and 'base64'. Since: - ksqldb 0.21

## KSqlDbRestApiClient.InsertIntoAsync
- added support for ```IEnumerable<T>``` properties #10

### Inserting empty arrays
- empty arrays are generated in the following way (workaround)
```SQL
ARRAY_REMOVE(ARRAY[0], 0))
```
```ARRAY[]``` is not yet supported

# v1.8.0
### KSqlDbRestApiClient
- DropTypeIfExistsAsync and DropTypeAsync - Removes a type alias from ksqlDB. If the IF EXISTS clause is present, the statement doesn't fail if the type doesn't exist.
- ToInsertStatement - Generates raw string Insert Into, but does not execute it.

### Operator BETWEEN
- KSqlOperatorExtensions - Between - Constrain a value to a specified range in a WHERE clause.
- KSqlOperatorExtensions - NotBetween - operator is used to indicate that a certain value must not be within a specified range, including boundaries.

# v1.7.0
- KPullSet - GetManyAsync - Pulls all values from the materialized view asynchronously and terminates. #6
- QbservableExtensions - ExplainAsync and ExplainAsStringAsync - Show the execution plan for a SQL expression, show the execution plan plus additional runtime information and metrics.

# v1.6.0
### KSqlDbRestApiClient
- CreateTypeAsync -  Create an alias for a complex type declaration. [#4 Complex Types](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB/issues/4)

- InsertIntoAsync for Complex types - `List<T>`, record, class and struct

### IN operator
- `IEnumerable<T>` and `IList<T>` Contains method in Where and Select clauses is interpreted as IN.  [#5 Use 'Contains()'](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB/issues/5)

FIX:
- IEnumerables are converted to ksql ARRAY not to a list of comma separated values

# v1.5.0
### QbservableExtensions
ObserveOn - Wraps the source sequence in order to run its observer callbacks on the specified scheduler.

SubscribeOn - Wraps the source sequence in order to run its subscription on the specified scheduler.

SubscribeAsync - Subscribes an element handler, an exception handler, and a completion handler to an qbservable stream and asynchronously returns the query id.

# v1.4.0
KSqlDbRestApiClient:
- TerminatePushQueryAsync - terminates push query by query id
- DropStreamAsync - Drops an existing stream.
- DropTableAsync - Drops an existing table.

# v1.3.0
KSqlDbRestApiClient:
- CreateSourceConnectorAsync - Create a new source connector in the Kafka Connect cluster with the configuration passed in the config parameter.
- CreateSinkConnectorAsync - Create a new sink connector in the Kafka Connect cluster with the configuration passed in the config parameter.

- GetTopicsAsync - lists the available topics in the Kafka cluster that ksqlDB is configured to connect to.
- GetAllTopicsAsync - lists all topics, including hidden topics.
- GetTopicsExtendedAsync - list of topics. Also displays consumer groups and their active consumer counts.
- GetAllTopicsExtendedAsync - list of all topics. Also displays consumer groups and their active consumer counts.

- GetQueriesAsync - Lists queries running in the cluster.
- QueriesResponse, Query

- TerminatePersistentQueryAsync - Terminate a persistent query. Persistent queries run continuously until they are explicitly terminated.

Scalar functions: ExtractJsonField, ConcatWS, Encode

HttpResponseMessageExtensions - added JsonSerializerOptions - PropertyNameCaseInsensitive = true

Bug fix:
- [#1](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB/issues/1) - [Pull Query] JsonException when a field contains a comma

# v1.2.0

KSqlDbRestApiClient:
- GetConnectorsAsync - List all connectors in the Connect cluster.
- DropConnectorAsync, DropConnectorIfExistsAsync - Drop a connector and delete it from the Connect cluster.

- ConnectorsResponse, Connector, HttpResponseMessageExtensions.ToConnectorsResponseAsync

- GetStreamsAsync - List the defined streams.
- StreamsResponse, Stream, StatementResponseBase

- GetTablesAsync - List the defined tables.
- TablesResponse, Table,

- scalar collection functions: AsMap, JsonArrayContains, MapKeys

# v1.1.0
- Pull queries Select extension method
- Push queries [WithOffsetResetPolicy extension method](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB#withoffsetresetpolicy---push-queries-extension-method-v110)
- CAST - [ToString](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB#cast---tostring-v110), [string to numeric types](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB#cast---convert-string-to-numeric-types-v110)
- scalar functions: Concat, ArraySort, ArrayUnion

# v1.0.0
- [Insert Values](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB/blob/main/README.md#insert-into-v100) - Produce a row into an existing stream or table

Great news are that Confluent added this package to their [documentation](https://github.com/confluentinc/ksql/pull/7520/files) as a contributed .NET client, so before wider adoption I decided to improve the API at cost of some breaking changes.
Except of one change all of them will be catched by the compiler. For more information see [Wiki](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB/blob/main/README.md#breaking-changes).

**Breaking changes.** In order to improve the v1.0 release the following methods and properties were renamed:

IKSqlDbRestApiClient interface:
```
| v0.11.0                       | v1.0.0                        |
|---------------------------------------------------------------|
| CreateTable<T>                | CreateTableAsync<T>           |
| CreateStream<T>               | CreateStreamAsync<T>          |
| CreateOrReplaceTable<T>       | CreateOrReplaceTableAsync<T>  |
| CreateOrReplaceStream<T>      | CreateOrReplaceStreamAsync<T> |
```

KSQL documentation refers to stream or table name in FROM as [from_item](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/select-push-query/)

```
IKSqlDBContext.CreateQuery<TEntity>(string streamName = null)
IKSqlDBContext.CreateQueryStream<TEntity>(string streamName = null)
```
streamName parameters were renamed:
```
IKSqlDBContext.CreateQuery<TEntity>(string fromItemName = null)
IKSqlDBContext.CreateQueryStream<TEntity>(string fromItemName = null)

QueryContext.StreamName was renamed QueryContext.FromItemName
Source.Of parameter streamName was renamed to fromItemName
KSqlDBContextOptions.ShouldPluralizeStreamName was renamed to ShouldPluralizeFromItemName
```

> ⚠ From version 1.0.0 the overriden from item names are pluralized, too. Join items are also affected by this breaking change.
This breaking change can cause runtime exceptions for users updating from lower versions. In case that you have never used custom singular from-item names, your code won't be affected.

# v0.11.0
- [CREATE STREAM](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-stream/) - fluent API
- [CREATE TABLE](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-table/) - fluent API

# v0.10.0
- [Pull queries](https://github.com/tomasfabian/Joker/wiki/Kafka.DotNet.ksqlDB---push-queries-LINQ-provider#pull-queries---createpullquerytentity-v0100) - IPullQueryProvider, IPullable, PullQueryExtensions
- Pull query [window bounds](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/select-pull-query/#example)

Fixes:
- fixed KSqlDbRestApiClient SendAsync set to defaultCompletionOption

# v0.9.0
[Create or replace stream/table as select](https://github.com/tomasfabian/Joker/wiki/Kafka.DotNet.ksqlDB---push-queries-LINQ-provider#v090-wip---preview):
- IKSqlDBStatementsContext - CreateStreamStatement, CreateOrReplaceStreamStatement, CreateTableStatement, CreateOrReplaceTableStatement
- CreateStatementExtensions - PartitionBy, ToStatementString
- WithOrAsClause, CreationMetadata
- ICreateStatement, CreateStatementExtensions

# v0.8.0
- scalar collection functions: ArrayMax, ArrayMin, ArrayRemove

Extensions:
- HttpResponseMessageExtensions - [ToStatementResponse](https://github.com/tomasfabian/Joker/wiki/Kafka.DotNet.ksqlDB---push-queries-LINQ-provider#httpresponsemessage-tostatementresponses-extension-v080)

### KSqlDbRestApiClient:
- ExecuteStatementAsync - The /ksql resource runs a sequence of SQL statements. All statements, except those starting with SELECT, can be run on this endpoint. To run SELECT statements use the /query endpoint.

### KSqlDbStatement
- [KSqlDbStatement](https://github.com/tomasfabian/Joker/wiki/Kafka.DotNet.ksqlDB---push-queries-LINQ-provider#ksqldbstatement-v080) allows you to set the statement, content encoding and the CommandSequenceNumber.

# v0.7.0:
- [operator precedence](https://github.com/tomasfabian/Joker/wiki/Kafka.DotNet.ksqlDB---push-queries-LINQ-provider#lexical-precedence-v070)
- fixed VisitNew with several binary expressions, all except the first were skipped
- [raw string KSQL query execution](https://github.com/tomasfabian/Joker/wiki/Kafka.DotNet.ksqlDB---push-queries-LINQ-provider#raw-string-ksql-query-execution-v070)
- scalar collection functions: ArrayIntersect, ArrayJoin, ArrayLength

# v0.6.0:
- netstandard 2.0 (.Net Framework etc)
- /Query endpoint (http 1.1)
- CASE - Select a condition from one or more expressions.

Added implementations:
- QueryParameters, KSqlDbContextOptionsBuilder
- KSqlDBContext.CreateQuery

Fixes:
- column alias in projections was not generated

# v0.5.0:
- Struct type
- Full Outer Join
- Numeric scalar functions - Entries Exp, GenerateSeries, GeoDistance, Ln, Sqrt
- Collection functions: ArrayContains, ArrayDistinct, ArrayExcept

# v0.4.0:
- Maps
- Deeply nested types (Maps, Arrays)
- logical operator NOT on columns
- aggregation function - Histogram

### Date and time functions
- DATETOSTRING, TIMESTAMPTOSTRING etc.

# v0.3.0:
- functions member access (variables for method arguments)
- Where is null, is not null
- dynamic function call (support not supported functions)
- destructure arrays (indexer), arrays length, new array

### ExtensionsMethods:
- LeftJoin
- Having - aggregations with column

#### Numeric functions
- Abs, Ceil, Floor, Random, Sign, Round

#### Aggregation functions
- EarliestByOffset, LatestByOffset, EarliestByOffsetAllowNulls, LatestByOffsetAllowNull
- TopK, TopKDistinct, LongCount, Count(col)
- EarliestByOffset - earliestN overload
- LatestByOffset - latestN overload
- CollectSet, CollectList, CountDistinct

# v0.2.0

### ExtensionsMethods:
- Having
- Window Session
- arithmetic operators
- KSqlFunctions - LIKE
- String functions - LPad, RPad, Trim, Len, Substring
- Aggregation functions - Min and Max
- Avg - Return the average value for a given column
- Inner Join
- TimeUnit milliseconds
- Source.of for inner join helper

### Fixes:
- parse single value for anonymous type - KSqlDbQueryStreamProvider bug fix

# v0.1.0
### ExtensionsMethods:
- AsAsyncEnumerable
- Sum Aggregation
- Tumbling window, Hopping window

- KSqlDBContext async disposition AsyncDisposableObject
- IKSqlGrouping

- Queries UCASE and LCASE

# v0.1.0-preview3

### Implementations:
- convert query visitor
- Record base type with RowTime property
- KSqlDBContext, KSqlDBContextOptions, QueryContext
- ServiceProvider

### ExtensionsMethods:
- GroupBy
- Count Aggregation

# v0.1.0-preview2
- KQuerySet was set to internal for maintanance reasons. Is KQueryStreamSet good enough for all push queries?

### ToObservable:
- ToObservable
- Subscribe overloads

# v0.1.0-preview1
### ksql provider:
- SELECT projections
- FROM entity type (KStream name)
- WHERE conditions (AND, OR)
- EMIT CHANGES
- LIMIT take linq extension method

### ExtensionsMethods:
- Subscribe

### Interfaces:
- IQbservableProvider
- ```IQbservable<TEntity>```

### Implementations:
- KSqlVisitor
- KSqlQueryGenerator - compiler
- KStreamSet, KQuerySet, KQueryStreamSet
- QbservableProvider
- ```KSqldbProvider<T>``` - ksqldb REST api provider for push queries (```KSqlDbQueryProvider<T>```, ```KSqlDbQueryStreamProvider<T>```)

# TODO:
- missing scalar functions https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/scalar-functions/#date-and-time
- - GRACE PERIOD on stream-stream joins ksqldb 0.23.1
