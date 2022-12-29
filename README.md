This package generates ksql queries from your .NET C# linq queries. You can filter, project, limit, etc. your push notifications server side with [ksqlDB push queries](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/streaming-endpoint/).
You can continually process computations over unbounded (theoretically never-ending) streams of data.
It also allows you to execute SQL [statements](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/) via the Rest API such as inserting records into streams and creating tables, types, etc. or executing admin operations such as listing streams.

[ksqlDB.RestApi.Client](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet) is a contribution to [Confluent ksqldb-clients](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-clients/)

[![main](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/actions/workflows/dotnetcore.yml/badge.svg?branch=main&event=push)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/actions/workflows/dotnetcore.yml/)

Install with NuGet package manager:
```
Install-Package ksqlDB.RestApi.Client
```
or with .NET CLI
```
dotnet add package ksqlDB.RestApi.Client
```
This adds a `<PackageReference>` to your csproj file, similar to the following:
```XML
<PackageReference Include="ksqlDB.RestApi.Client" Version="2.3.0" />
```

Alternative option is to use [Protobuf content type](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/protobuf.md).

The following example can be tried with a [.NET interactive Notebook](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/Notebooks):

```C#
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;

var ksqlDbUrl = @"http:\\localhost:8088";

var contextOptions = new KSqlDBContextOptions(ksqlDbUrl)
{
  ShouldPluralizeFromItemName = true
};

await using var context = new KSqlDBContext(contextOptions);

using var disposable = context.CreateQueryStream<Tweet>()
  .WithOffsetResetPolicy(AutoOffsetReset.Latest)
  .Where(p => p.Message != "Hello world" || p.Id == 1)
  .Select(l => new { l.Message, l.Id })
  .Take(2)
  .Subscribe(tweetMessage =>
  {
    Console.WriteLine($"{nameof(Tweet)}: {tweetMessage.Id} - {tweetMessage.Message}");
  }, error => { Console.WriteLine($"Exception: {error.Message}"); }, () => Console.WriteLine("Completed"));

Console.WriteLine("Press any key to stop the subscription");

Console.ReadKey();

public class Tweet : Record
{
  public int Id { get; set; }

  public string Message { get; set; }
}
```

LINQ code written in C# from the sample is equivalent to this ksql query:
```SQL
SELECT Message, Id
  FROM Tweets
 WHERE Message != 'Hello world' OR Id = 1 
  EMIT CHANGES 
 LIMIT 2;
```

In the above mentioned code snippet everything runs server side except of the ```IQbservable<TEntity>.Subscribe``` method. It subscribes to your ksqlDB stream created in the following manner:

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.Api.Client.Samples.Models;

EntityCreationMetadata metadata = new()
{
  KafkaTopic = nameof(Tweet),
  Partitions = 1,
  Replicas = 1
};

var httpClient = new HttpClient()
{
  BaseAddress = new Uri(@"http:\\localhost:8088")
};

var httpClientFactory = new HttpClientFactory(httpClient);
var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata);
```

CreateOrReplaceStreamAsync executes the following statement:
```SQL
CREATE OR REPLACE STREAM Tweets (
	Id INT,
	Message VARCHAR
) WITH ( KAFKA_TOPIC='Tweet', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );
```

Run the following insert statements to stream some messages with your ksqldb-cli
```
docker exec -it $(docker ps -q -f name=ksqldb-cli) ksql http://ksqldb-server:8088
```
```SQL
INSERT INTO tweets (id, message) VALUES (1, 'Hello world');
INSERT INTO tweets (id, message) VALUES (2, 'ksqlDB rulez!');
```

or insert a record from C#:
```C#
var ksqlDbUrl = @"http:\\localhost:8088";

var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

var responseMessage = await new KSqlDbRestApiClient(httpClientFactory)
  .InsertIntoAsync(new Tweet { Id = 2, Message = "ksqlDB rulez!" });
```

or with KSqlDbContext:

```C#
await using var context = new KSqlDBContext(ksqlDbUrl);

context.Add(new Tweet { Id = 1, Message = "Hello world" });
context.Add(new Tweet { Id = 2, Message = "ksqlDB rulez!" });

var saveChangesResponse = await context.SaveChangesAsync();
```

Sample project can be found under [Samples](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/ksqlDB.RestApi.Client.Sample) solution folder in ksqlDB.RestApi.Client.sln 


**External dependencies:**
- [kafka broker](https://kafka.apache.org/intro) and [ksqlDB-server](https://ksqldb.io/overview.html) 0.14.0
- the solution requires [Docker desktop](https://www.docker.com/products/docker-desktop) and Visual Studio 2019
- [.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

Clone the repository
```
git clone https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet.git
```

CD to [Samples](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/ksqlDB.RestApi.Client.Sample)
```
CD Samples\ksqlDB.RestApi.Client.Sample\
```

run in command line:

```docker compose up -d```

**AspNet Blazor server side sample:**

- set docker-compose.csproj as startup project in InsideOut.sln for an embedded Kafka connect integration and stream processing examples.

# Setting query parameters
Default settings:
'auto.offset.reset' is set to 'earliest' by default. 
New parameters could be added or existing ones changed in the following manner:
```C#
var contextOptions = new KSqlDBContextOptions(@"http:\\localhost:8088");

contextOptions.QueryStreamParameters["auto.offset.reset"] = "latest";
```

### Overriding stream names
Stream names are generated based on the generic record types. They are pluralized with Pluralize.NET package

**By default the generated from item names such as stream and table names are pluralized**. This behaviour could be switched off with the following `ShouldPluralizeStreamName` configuration. 
> ⚠  KSqlDBContextOptions.ShouldPluralizeStreamName was renamed to ShouldPluralizeFromItemName

```C#
context.CreateQueryStream<Person>();
```
```SQL
FROM People
```
This can be disabled:
```C#
var contextOptions = new KSqlDBContextOptions(@"http:\\localhost:8088")
{
  ShouldPluralizeStreamName = false
};

new KSqlDBContext(contextOptions).CreateQueryStream<Person>();
```
```SQL
FROM Person
```

In v1.0 was ShouldPluralizeStreamName renamed to **ShouldPluralizeFromItemName**
```C#
var contextOptions = new KSqlDBContextOptions(@"http:\\localhost:8088")
{
  ShouldPluralizeFromItemName = false
};
```

Setting an arbitrary stream name (from_item name):
```C#
context.CreateQueryStream<Tweet>("custom_topic_name");
```
```SQL
FROM custom_topic_name
```

# ```IQbservable<T>``` extension methods
<img src="https://www.codeproject.com/KB/cs/646361/WhatHowWhere.jpg" />

List of supported [push query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md) extension methods:
- [Select](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#select)
- [Take (LIMIT)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#take-limit)
- [Subscribe](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#subscribe)
- [ToObservable](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#toobservable)
- [ToAsyncEnumerable](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#toasyncenumerable)
- [ToQueryString](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#toquerystring)
- [ExplainAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#explainasync)
- [SubscribeAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#subscribeasync)
- [SubscribeOn](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#subscribeon)
- [ObserveOn](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#observeon)
- [ToStatementString](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#tostatementstring)
- [WithOffsetResetPolicy](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#withoffsetresetpolicy---push-queries-extension-method)
- [Window bounds](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#window-bounds)
- [Raw string KSQL query execution](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#raw-string-ksql-query-execution)

- [IKSqlGrouping.Source](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/push_queries.md#iksqlgroupingsource)

### Select (v0.1.0)
Projects each element of a stream into a new form.
```C#
context.CreateQueryStream<Tweet>()
  .Select(l => new { l.RowTime, l.Message });
```
Omitting select is equivalent to SELECT *

### Where (v0.1.0)
Filters a stream of values based on a predicate.
```C#
context.CreateQueryStream<Tweet>()
  .Where(p => p.Message != "Hello world" || p.Id == 1)
  .Where(p => p.RowTime >= 1510923225000);
```
Multiple Where statements are joined with AND operator. 
```KSQL
SELECT * FROM Tweets
WHERE Message != 'Hello world' OR Id = 1 AND RowTime >= 1510923225000
EMIT CHANGES;
```

Supported operators are:

|   ksql   |           meaning           |  c#  |
|:--------:|:---------------------------:|:----:|
| =        | is equal to                 | ==   |
| != or <> | is not equal to             | !=   |
| <        | is less than                | <    |
| <=       | is less than or equal to    | <=   |
| >        | is greater than             | >    |
| >=       | is greater than or equal to | >=   |
| AND      | logical AND                 | &&   |
| OR       | logical OR                  | \|\| |
| NOT      | logical NOT                 |  !   |

### Aggregation functions
List of supported ksqldb [aggregation functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md):
- [GROUP BY](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#groupby)
- [MIN, MAX](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#min-and-max)
- [AVG](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#avg)
- [COUNT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#count)
- [COLLECT_LIST, COLLECT_SET, EARLIEST_BY_OFFSET, LATEST_BY_OFFSET](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#collect_list-collect_set-earliest_by_offset-latest_by_offset)
- [SUM](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#sum)
- COUNT_DISTINCT
- HISTOGRAM
- [TOPK,TOPKDISTINCT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#topk-topkdistinct-longcount-countcolumn)
- [COLLECTSET, COLLECTLIST, COUNT_DISTINCT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#collectset-collectlist-countdistinct)
- [HAVING](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#having)

- [TimeWindows - EMIT FINAL](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#timewindows---emit-final)
- [WindowedBy](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#windowedby)
  - [Session Window](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#session-window)
  - [Tumbling Window](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#tumbling-window)
  - [Hopping Window](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/aggregations.md#hopping-window)

[Rest api reference](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/)

[Some KSql function examples can be found here](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/wiki/KSql-functions)

# TFM netstandard 2.0 (.Net Framework, NetCoreApp 2.0 etc.)
netstandard 2.0 does not support Http 2.0. Due to this ```IKSqlDBContext.CreateQueryStream<TEntity>``` is not exposed at the current version. 
For these reasons ```IKSqlDBContext.CreateQuery<TEntity>``` was introduced to provide the same functionality via Http 1.1. 

Redundant brackets are not reduced in the current version

**Data definititions:**
- [Headers](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/data_definitions.md#access-record-header-data-v160)

**List of supported data types:**
- [Supported data types mapping](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/data_types.md#supported-data-types-mapping)
- [Structs](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/data_types.md#structs)
- [Maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/data_types.md#maps)
- [Time types DATE, TIME AND TIMESTAMP](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/data_types.md#time-types-date-time-and-timestamp)
- [System.GUID as ksqldb VARCHAR type](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/data_types.md#systemguid-as-ksqldb-varchar-type-v240)

**List of supported Joins:**
- [RightJoin](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/joins.md#rightjoin)
- [Full Outer Join](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/joins.md#full-outer-join)
- [Left Join](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/joins.md#leftjoin---left-outer)
- [Inner Joins](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/joins.md#inner-joins)
- [Multiple joins with query comprehension syntax (GroupJoin, SelectMany, DefaultIfEmpty)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/joins.md#multiple-joins-with-query-comprehension-syntax-groupjoin-selectmany-defaultifempty)

List of supported [pull query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/pull_queries.md) extension methods:
- [Take (LIMIT)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/pull_queries.md#pull-query-take-extension-method-limit)
- [FirstOrDefaultAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/pull_queries.md#ipullabletfirstordefaultasync-v100)
- [GetManyAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/pull_queries.md#getmanyasync)
- [CreatePullQuery](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/pull_queries.md#createpullquerytentity-v100)
- [ExecutePullQuery](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/pull_queries.md#pull-queries---executepullquery)

**List of supported ksqlDB SQL statements:**
- [Pause and resume persistent qeries](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#pause-and-resume-persistent-qeries-v250)
- [InsertProperties.UseInstanceType](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#insertpropertiesuseinstancetype)
- [Added support for extracting field names and values (for insert and select statements)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#added-support-for-extracting-field-names-and-values-for-insert-and-select-statements)
- [AssertTopicExistsAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#asserttopicexistsasync-and-asserttopicnotexistsasync)
- [AssertSchemaExistsAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#assertschemaexistsasync-and-assertschemanotexistsasync)
- [Rename stream or table column names with the `JsonPropertyNameAttribute`](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#rename-stream-or-table-column-names-with-the-jsonpropertynameattribute)
- [IKSqlDbRestApiClient CreateSourceStreamAsync and CreateSourceTableAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#iksqldbrestapiclient-createsourcestreamasync-and-createsourcetableasync)
- [InsertProperties.IncludeReadOnlyProperties](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#insertpropertiesincludereadonlyproperties)
- [InsertIntoAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#ksqldbrestapiclientinsertintoasync)
- [Connectors](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#connectors)
- [Drop a stream](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#drop-a-stream)
- [Drop type](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#droping-types)
- [Creating types](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#createtypeasync)
- [ExecuteStatementAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#executestatementasync-extension-method)
- [PartitionBy](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#partitionby)
- [Terminate push queries](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#terminate-push-queries)
- [Drop a table](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#drop-a-table)
- [Creating connectors](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#creating-connectors)
- [Get topics](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#get-topics)
- [Getting queries and termination of persistent queries](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#getting-queries-and-termination-of-persistent-queries)
- [ExecuteStatementAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#executestatementasync)
- [Create or replace table statements](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#create-or-replace-table-statements)
- [Creating streams and tables](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#creating-streams-and-tables)
- [Get streams](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#get-streams)
- [Get tables](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/statements.md#get-tables)

**KSqlDbContext**
- [CreateQueryStream](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#createquerystream)
- [CreateQuery](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#createquery)
- [AddDbContext and AddDbContextFactory](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#ksqldbservicecollectionextensions---adddbcontext-and-adddbcontextfactory)
- [Logging info and ConfigureKSqlDb](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#logging-info-and-configureksqldb)
- [Basic auth](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#basic-auth)
- [Add and SaveChangesAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#iksqldbcontext-add-and-savechangesasync)
- [KSqlDbContextOptionsBuilder](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#ksqldbcontextoptionsbuilder)

**Config:**
- [KSqlDbContextOptionsBuilder.ReplaceHttpClient](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/config.md#ksqldbcontextoptionsbuilderreplacehttpclient)
- [ProcessingGuarantee](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/config.md#processingguarantee-enum)

**Operators**
- [Operator LIKE](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#operator-like---stringstartswith-stringendswith-stringcontains)
- [Operator IN](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#operator-in---ienumerablet-and-ilistt-contains)
- [Operator BETWEEN](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#operator-not-between)
- [CASE](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#case)
- [Arithmetic operations on columns](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#arithmetic-operations-on-columns)
- [Lexical precedence](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#lexical-precedence)
- [WHERE IS NULL, IS NOT NULL](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#where-is-null-is-not-null)

**Miscelenaous:**
- [Change data capture](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/cdc.md)
- [List of breaking changes](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/breaking_changes.md)
- [Operators](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md)
- [Invocation functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#improved-invocation-function-extensions)
- [SetJsonSerializerOptions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/config.md#setjsonserializeroptions)
- [Kafka stream processing example](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/stream_processing_.md)

**Functions**
- [String functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#string-functions)
- [Numeric functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#numeric-functions)
- [Date and time functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#date-and-time-functions)
- [Lambda functions (Invocation functions) - Maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#lambda-functions-invocation-functions---maps)
  - [Transform maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#transform-maps)
  - [Filter maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#filter-maps)
  - [Reduce maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/functions.md#reduce-maps)

# LinqPad samples
[Push Query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/ksqlDB.RestApi.Client.LinqPad/ksqlDB.RestApi.Client.linq)

[Pull Query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/ksqlDB.RestApi.Client.LinqPad/ksqlDB.RestApi.Client.pull-query.linq)

# Nuget
https://www.nuget.org/packages/ksqlDB.RestApi.Client/

**TODO:**
- [CREATE TABLE AS SELECT](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/create-table-as-select/) - EMIT output_refinement
- rest of the [ksql query syntax](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/select-push-query/) (supported operators etc.)

# ksqldb links
[Scalar functions](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/scalar-functions/#as_value)

[Aggregation functions](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/)

[Push query](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/select-push-query/)

# Acknowledgements:
- [ksql](https://github.com/confluentinc/ksql)

- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/)
- [Pluralize.NET](https://www.nuget.org/packages/Pluralize.NET/)
- [System.Interactive.Async](https://www.nuget.org/packages/System.Interactive.Async/)
- [System.Reactive](https://www.nuget.org/packages/System.Reactive/)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json/)

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/tomasfabian)
