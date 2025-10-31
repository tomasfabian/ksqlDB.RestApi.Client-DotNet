This package enables seamless integration of **KSQL** push and pull queries with LINQ queries in your .NET C# applications.
It allows you to perform server-side operations such as filtering, projection, limiting, and more directly on push notifications using [ksqlDB push queries](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-rest-api/streaming-endpoint/).
This facilitates continuous processing of computations over unbounded, potentially never-ending, streams of data.

In addition, the package supports executing SQL [statements](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/) via the REST API. You can use it to insert records into streams, create tables and types, and perform administrative tasks such as listing available streams.

[ksqlDB.RestApi.Client](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet) is a contribution to [Confluent ksqldb-clients](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-clients/)

[![main](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/actions/workflows/dotnet.yml/badge.svg?branch=main&event=push)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/actions/workflows/dotnet.yml/)

Install with **NuGet** package manager:
```
Install-Package ksqlDb.RestApi.Client
```
or with .NET CLI
```
dotnet add package ksqlDb.RestApi.Client
```
This adds a `<PackageReference>` to your csproj file, similar to the following:
```XML
<PackageReference Include="ksqlDb.RestApi.Client" Version="7.0.0" />
```

Alternative option is to use [Protobuf content type](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/protobuf.md):
```
dotnet add package ksqlDb.RestApi.Client.ProtoBuf
```

Feel free to experiment with the following example in a [.NET interactive Notebook](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/Notebooks):

```C#
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;

var ksqlDbUrl = @"http://localhost:8088";

var contextOptions = new KSqlDBContextOptions(ksqlDbUrl)
{
  ShouldPluralizeFromItemName = true
};

await using var context = new KSqlDBContext(contextOptions);

using var subscription = context.CreatePushQuery<Tweet>()
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
```

```C#
public class Tweet : Record
{
  public int Id { get; set; }

  public string Message { get; set; }
}
```

An entity class in **ksqlDB.RestApi.Client** represents the structure of a table or stream.
An instance of the class represents a record in that stream or table while properties are mapped to columns respectively.

LINQ code written in C# from the sample is equivalent to this KSQL query:
```SQL
SELECT Message, Id
  FROM Tweets
 WHERE Message != 'Hello world' OR Id = 1
  EMIT CHANGES
 LIMIT 2;
```

In the provided C# code snippet, most of the code executes on the server side except for the `IQbservable<TEntity>.Subscribe` extension method. This method is responsible for subscribing to your `ksqlDB` stream, which is created using the following approach:

```C#
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.Api.Client.Samples.Models;

EntityCreationMetadata metadata = new(kafkaTopic: nameof(Tweet))
{
  Partitions = 3,
  Replicas = 3
};

var httpClient = new HttpClient()
{
  BaseAddress = new Uri(@"http://localhost:8088")
};

var httpClientFactory = new HttpClientFactory(httpClient);
var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<Tweet>(metadata);
```

`CreateOrReplaceStreamAsync` executes the following statement:
```SQL
CREATE OR REPLACE STREAM Tweets (
  Id INT,
  Message VARCHAR
) WITH ( KAFKA_TOPIC='Tweet', VALUE_FORMAT='Json', PARTITIONS='3', REPLICAS='3' );
```

Execute the following insert statements to **publish messages** using your `ksqldb-cli`
```
docker exec -it $(docker ps -q -f name=ksqldb-cli) ksql http://ksqldb-server:8088
```
```SQL
INSERT INTO tweets (id, message) VALUES (1, 'Hello world');
INSERT INTO tweets (id, message) VALUES (2, 'ksqlDB rulez!');
```

or insert a record from C#:
```C#
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

Sample projects can be found under [Samples](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/ksqlDB.RestApi.Client.Sample) solution folder in ksqlDB.RestApi.Client.sln


**External dependencies:**
- [kafka broker](https://kafka.apache.org/intro) and [ksqlDB-server](https://ksqldb.io/overview.html) 0.14.0
- the solution requires [Docker desktop](https://www.docker.com/products/docker-desktop) and Visual Studio 2019
- [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

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

In [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor), the application logic and UI rendering occur on the server. The client's web browser receives updates and UI changes through a **SignalR** connection.
This ensures smooth integration with the `ksqlDB.RestApi.Client` library, allowing the **Apache Kafka broker** and **ksqlDB** to remain hidden from direct exposure to clients.
The **server-side Blazor** application communicates with `ksqlDB` using the `ksqlDB.RestApi.Client`.
Whenever an event in `ksqlDB` occurs, the server-side Blazor app responds and signals the UI in the client's browser to update. This setup allows a smooth and continuous update flow, creating a real-time, reactive user interface.

- set `docker-compose.csproj` as startup project in [InsideOut.sln](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/InsideOut) for embedded Kafka connect integration and stream processing examples.

# ```IQbservable<T>``` extension methods
As depicted below `IObservable<T>` is the dual of `IEnumerable<T>` and `IQbservable<T>` is the dual of `IQueryable<T>`. In all four cases LINQ providers are using deferred execution.
While the first two are executed locally the latter two are executed server side. The server side execution is possible thanks to traversing **AST**s (Abstract Syntax Trees) with visitors. The `KSqlDbProvider` will create the **KSQL syntax** for you from **expression trees** and pass it along to ksqlDB.

Both `IObservable<T>` and `IQbservable<T>` represent **push-based** sequences of asynchronous and potentially infinite events, while `IEnumerable<T>` and `IQueryable<T>` represent collections or **pull-based** sequences of items that can be iterated or queried, respectively.

<img src="https://i.sstatic.net/2DAqv.jpg" />

List of supported [push query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md) extension methods:
- [Select](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#select)
- [Where](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#where)
- [Take (LIMIT)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#take-limit)
- [Subscribe](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#subscribe)
- [ToObservable](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#toobservable)
- [ToAsyncEnumerable](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#toasyncenumerable)
- [ToQueryString](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#getting-the-generated-ksql)
- [ExplainAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#explainasync)
- [SubscribeAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#subscribeasync)
- [SubscribeOn](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#subscribeon)
- [ObserveOn](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#observeon)
- [ToStatementString](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#tostatementstring)
- [WithOffsetResetPolicy](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#withoffsetresetpolicy---push-queries-extension-method)
- [Window bounds](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#window-bounds)
- [Raw string KSQL query execution](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#raw-string-ksql-query-execution)

- [IKSqlGrouping.Source](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/push_queries.md#iksqlgroupingsource)

# Register the KSqlDbContext
`IKSqlDBContext` and `IKSqlDbRestApiClient` can be provided with **dependency injection**. These services can be registered during app startup and components that require these services, are provided with these services via constructor parameters.

To register `KSqlDbContext` as a service, open `Program.cs`, and add the lines to the `ConfigureServices` method shown below or see some more details in [the workshop](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/wiki/ksqlDB.RestApi.Client-workshop):

```
using ksqlDB.RestApi.Client.Sensors;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.Sensors.KSqlDb;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
      var ksqlDbUrl = @"http://localhost:8088";

      services.AddDbContext<ISensorsKSqlDbContext, SensorsKSqlDbContext>(
        options =>
        {
          var setupParameters = options.UseKSqlDb(ksqlDbUrl);

          setupParameters.Options.ShouldPluralizeFromItemName = false;
          setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);

        }, ServiceLifetime.Transient, restApiLifetime: ServiceLifetime.Transient);
    })
    .Build();

await host.RunAsync();
```

# KSqlDbContextOptions builder
To modify parameters or introduce new ones, utilize the following approach:

```C#
var contextOptions = new KSqlDbContextOptionsBuilder()
  .UseKSqlDb("http://localhost:8088)
  .SetBasicAuthCredentials("fred", "flinstone")
  .SetJsonSerializerOptions(jsonOptions =>
  {
    jsonOptions.IgnoreReadOnlyFields = true;
  })
  .SetAutoOffsetReset(AutoOffsetReset.Latest)
  .SetProcessingGuarantee(ProcessingGuarantee.ExactlyOnce)
  .SetIdentifierEscaping(IdentifierEscaping.Keywords)
  .SetupPushQuery(options =>
  {
    options.Properties["ksql.query.push.v2.enabled"] = "true";
  })
  .Options;
```

This code initializes a `KSqlDbContextOptionsBuilder` to configure settings for a `ksqlDB` context. Here's a breakdown of the configurations:

- `UseKSqlDb("http://localhost:8088")`: Specifies the **URL** of the `ksqlDB` server.
- `SetBasicAuthCredentials("fred", "flinstone")`: Sets the basic authentication credentials (username and password) for accessing the `ksqlDB` server.
- `SetJsonSerializerOptions(jsonOptions => { ... })`: Configures JSON serialization options, such as ignoring read-only fields.
- `SetAutoOffsetReset(AutoOffsetReset.Latest)`: Sets the offset reset behavior to start consuming messages from the **latest** available when no committed offset is found. By default, 'auto.offset.reset' is configured to 'earliest'.
- `SetProcessingGuarantee(ProcessingGuarantee.ExactlyOnce)`: Specifies the processing guarantee as **exactly-once** semantics.
- `SetIdentifierEscaping(IdentifierEscaping.Keywords)`: Escapes identifiers such as table and column names that are SQL keywords.
- `SetupPushQuery(options => { ... })`: Configures push query options, specifically enabling KSQL query push version 2.

Finally, `.Options` returns the configured options for the `ksqlDB` context.

### Overriding stream names
Stream names are generated based on the generic record types. They are pluralized with Pluralize.NET package.

**By default the generated from item names such as stream and table names are pluralized**. This behavior could be switched off with the following `ShouldPluralizeStreamName` configuration.

```C#
context.CreatePushQuery<Person>();
```
```SQL
FROM People
```
This can be disabled:
```C#
var contextOptions = new KSqlDBContextOptions(@"http://localhost:8088")
{
  ShouldPluralizeFromItemName = false
};

new KSqlDBContext(contextOptions).CreatePushQuery<Person>();
```
```SQL
FROM Person
```

Setting an arbitrary stream name (from_item name):
```C#
context.CreatePushQuery<Tweet>("custom_topic_name");
```
```SQL
FROM custom_topic_name
```

# KSqlDbRestApiClient
The `KSqlDbRestApiClient` supports various operations such as executing KSQL statements, inserting data into streams asynchronously, creating, listing or dropping entities, and managing KSQL connectors.

```C#
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDB.RestApi.Client.Samples.Models.Movies;

public static async Task ExecuteAsync(CancellationToken cancellationToken = default)
{
  var httpClient = new HttpClient()
  {
    BaseAddress = new Uri("http://localhost:8088")
  };
  var httpClientFactory = new HttpClientFactory(httpClient);
  var kSqlDbRestApiClient = new KSqlDbRestApiClient(httpClientFactory);

  EntityCreationMetadata entityCreationMetadata = new(kafkaTopic: "companyname.movies")
  {
    Partitions = 3,
    Replicas = 3,
    ValueFormat = SerializationFormats.Json,
    IdentifierEscaping = IdentifierEscaping.Keywords
  };

  var httpResponseMessage = await kSqlDbRestApiClient.CreateOrReplaceTableAsync<Movie>(entityCreationMetadata, cancellationToken);
  var responses = await httpResponseMessage.ToStatementResponsesAsync();
  Console.WriteLine($"Create or replace table response: {responses[0].CommandStatus!.Message}");

  Console.WriteLine($"{Environment.NewLine}Available tables:");
  var tablesResponses = await kSqlDbRestApiClient.GetTablesAsync(cancellationToken);
  Console.WriteLine(string.Join(', ', tablesResponses[0].Tables!.Select(c => c.Name)));

  var dropProperties = new DropFromItemProperties
  {
    UseIfExistsClause = true,
    DeleteTopic = true,
    IdentifierEscaping = IdentifierEscaping.Keywords
  };
  httpResponseMessage = await kSqlDbRestApiClient.DropTableAsync<Movie>(dropProperties, cancellationToken: cancellationToken);
  tablesResponses = await kSqlDbRestApiClient.GetTablesAsync(cancellationToken);
}
```

```C#
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

public class Movie : Record
{
  [Key]
  public int Id { get; set; }
  public string Title { get; set; } = null!;
}
```

# Model builder
By leveraging the `ksqlDb.RestApi.Client` fluent API model builder, you can streamline the configuration process, improve code readability, and mitigate issues related to code regeneration by keeping configuration logic separate from generated POCOs.

```C#
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;

ModelBuilder modelBuilder = new();

var decimalTypeConvention = new DecimalTypeConvention(14, 14);

modelBuilder.AddConvention(decimalTypeConvention);

modelBuilder.Entity<Payment>()
  .Property(b => b.Amount)
  .Decimal(precision: 10, scale: 2);

modelBuilder.Entity<Payment>()
  .Property(b => b.Description)
  .HasColumnName("Desc");

modelBuilder.Entity<Account>()
  .HasKey(c => c.Id);

modelBuilder.Entity<Account>()
  .Property(b => b.Secret)
  .Ignore();
```

C# entity definitions:

```C#
record Payment
{
  public string Id { get; set; } = null!;
  public decimal Amount { get; set; }
  public string Description { get; set; } = null!;
}

record Account
{
  public string Id { get; set; } = null!;
  public decimal Balance { get; set; }
  public string Secret { get; set; }
}
```

Usage with ksqlDB REST API Client:

```C#
var kSqlDbRestApiClient = new KSqlDbRestApiClient(httpClientFactory, modelBuilder);
await kSqlDbRestApiClient.CreateTypeAsync<Payment>(cancellationToken);

var entityCreationMetadata = new EntityCreationMetadata(kafkaTopic: nameof(Account), partitions: 3)
{
  Replicas = 3
};
responseMessage = await restApiProvider.CreateTableAsync<Account>(entityCreationMetadata, true, cancellationToken);
```

Generated KSQL:

```SQL
CREATE TYPE Payment AS STRUCT<Id VARCHAR, Amount DECIMAL(10,2), Desc VARCHAR>;

CREATE TABLE IF NOT EXISTS Accounts (
	Id VARCHAR PRIMARY KEY,
	Balance DECIMAL(14,14)
) WITH ( KAFKA_TOPIC='Account', VALUE_FORMAT='Json', PARTITIONS='3', REPLICAS='3' );
```

The `Description` property within the `Payment` type has been customized to override the resulting column name as "Desc".
Additionally, the `Id` property within the `Account` table has been designated as the **primary key**, while the `Secret` property is disregarded during code generation.

### Aggregation functions
List of supported ksqldb [aggregation functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md):
- [GROUP BY](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#groupby)
- [MIN, MAX](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#min-and-max)
- [AVG](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#avg)
- [COUNT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#count)
- [COLLECT_LIST, COLLECT_SET](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#collect_list-collect_set)
- [EARLIEST_BY_OFFSET, LATEST_BY_OFFSET](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#earliestbyoffset-latestbyoffset-earliestbyoffsetallownulls-latestbyoffsetallownull)
- [SUM](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#sum)
- [TOPK,TOPKDISTINCT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#topk-topkdistinct-longcount-countcolumn)
- [COUNT_DISTINCT](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#countdistinct)
- [HAVING](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#having)

- [TimeWindows - EMIT FINAL](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#timewindows---emit-final)
- [WindowedBy](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#windowedby)
  - [Session Window](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#session-window)
  - [Tumbling Window](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#tumbling-window)
  - [Hopping Window](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/aggregations.md#hopping-window)

[Rest API reference](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/aggregate-functions/)

**List of supported data types:**
- [Supported data types mapping](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/data_types.md#supported-data-types-mapping)
- [Structs](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/data_types.md#structs)
- [Maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/data_types.md#maps)
- [Time types DATE, TIME AND TIMESTAMP](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/data_types.md#time-types-date-time-and-timestamp)
- [System.GUID as ksqldb VARCHAR type](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/data_types.md#systemguid-as-ksqldb-varchar-type-v240)

**List of supported Joins:**
- [RightJoin](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/joins.md#rightjoin)
- [Full Outer Join](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/joins.md#full-outer-join)
- [Left Join](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/joins.md#leftjoin---left-outer)
- [Inner Joins](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/joins.md#inner-joins)
- [Multiple joins with query comprehension syntax (GroupJoin, SelectMany, DefaultIfEmpty)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/joins.md#multiple-joins-with-query-comprehension-syntax-groupjoin-selectmany-defaultifempty)

List of supported [pull query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/pull_queries.md) extension methods:
- [Take (LIMIT)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/pull_queries.md#pull-query-take-extension-method-limit)
- [FirstOrDefaultAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/pull_queries.md#ipullabletfirstordefaultasync-v100)
- [GetManyAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/pull_queries.md#getmanyasync)
- [CreatePullQuery](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/pull_queries.md#createpullquerytentity-v100)
- [ExecutePullQuery](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/pull_queries.md#pull-queries---executepullquery)

**List of supported ksqlDB SQL statements:**
- [Pause and resume persistent qeries](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#pause-and-resume-persistent-queries)
- [Added support for extracting field names and values (for insert and select statements)](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#added-support-for-extracting-field-names-and-values-for-insert-and-select-statements)
- [Assert topics](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#assert-topics)
- [Assert schemas](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#assert-schemas)
- [Rename stream or table column names with the `JsonPropertyNameAttribute`](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#rename-stream-or-table-column-names-with-the-jsonpropertynameattribute)
- [Create source streams and tables](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#createsourcestreamasync-and-createsourcetableasync)
- [InsertIntoAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#insertintoasync)
- [Connectors](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#connectors)
- [Drop a stream](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#drop-a-stream)
- [Drop type](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#droping-types)
- [Creating types](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#create-types)
- [Execute statement async](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#executestatementasync-extension-method)
- [PartitionBy](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#partitionby)
- [Terminate push queries](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#terminate-push-queries)
- [Drop a table](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#drop-a-table)
- [Creating connectors](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#creating-connectors)
- [Get topics](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#get-topics)
- [Getting queries and termination of persistent queries](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#getting-queries-and-termination-of-persistent-queries)
- [Execute statements](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#executestatementasync)
- [Create or replace table statements](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#create-or-replace-table-statements)
- [Creating streams and tables](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#creating-streams-and-tables)
- [Get streams](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#get-streams)
- [Get tables](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbrestapiclient.md#get-tables)

**KSqlDbContext**
- [Dependency injection with ServicesCollection](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/4e6487dbf201f4318da88707d62e1a75c6cef402/docs/ksqldbcontext.md#logging-info-and-configureksqldb)
- [Creating query streams](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbcontext.md#creating-query-streams)
- [Creating queries](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbcontext.md#creating-queries)
- [AddDbContext and AddDbContextFactory](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbcontext.md#ksqldbservicecollectionextensions---adddbcontext-and-adddbcontextfactory)
- [Logging info and ConfigureKSqlDb](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbcontext.md#logging-info-and-configureksqldb)
- [Basic auth](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbcontext.md#basic-auth)
- [Add and SaveChangesAsync](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbcontext.md#add-and-savechangesasync)
- [KSqlDbContextOptionsBuilder](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/ksqldbcontext.md#ksqldbcontextoptionsbuilder)

**Config**
- [Fluent API - Model builder](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/modelbuilder.md)
- [Bearer token authentication](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/config.md#bearer-token-authentication)
- [Replacing HttpClient](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/config.md#ksqldbcontextoptionsbuilderreplacehttpclient)
- [Processing guarantees](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/config.md#processingguarantee-enum)

**Operators**
- [Operator LIKE](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md#operator-like---stringstartswith-stringendswith-stringcontains)
- [Operator IN](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md#operator-in---ienumerablet-and-ilistt-contains)
- [Operator BETWEEN](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md#operator-not-between)
- [Operator CASE](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md#case)
- [Arithmetic operations on columns](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md#arithmetic-operations-on-columns)
- [Lexical precedence](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md#lexical-precedence)
- [WHERE IS NULL, IS NOT NULL](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md#where-is-null-is-not-null)

**Data definitions**
- [Headers](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/data_definitions.md#access-record-header-data-v160)

**Miscelenaous**
- [Change data capture](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/cdc.md)
- [List of breaking changes](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/breaking_changes.md)
- [Operators](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/operators.md)
- [Invocation functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#improved-invocation-function-extensions)
- [Setting JsonSerializerOptions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/config.md#setjsonserializeroptions)
- [Kafka stream processing example](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/stream_processing.md)
- [ksqlDB streams and tables](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/streams_and_tables.md)

**Functions**
- [String functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#string-functions)
- [Numeric functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#numeric-functions)
- [Date and time functions](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#date-and-time-functions)
- [Lambda functions (Invocation functions) - Maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#lambda-functions-invocation-functions---maps)
  - [Transform maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#transform-maps)
  - [Filter maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#filter-maps)
  - [Reduce maps](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/docs/functions.md#reduce-maps)

# LinqPad samples
[Push Query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/ksqlDB.RestApi.Client.LinqPad/ksqlDB.RestApi.Client.linq)

[Pull Query](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/tree/main/Samples/ksqlDB.RestApi.Client.LinqPad/ksqlDB.RestApi.Client.pull-query.linq)

# Nuget
https://www.nuget.org/packages/ksqlDB.RestApi.Client/

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
