# v1.2.0-rc.1 (WIP)

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
- CreateQueryStream options parameter
- BETWEEN
- CAST