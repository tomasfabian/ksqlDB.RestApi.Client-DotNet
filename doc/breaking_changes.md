# Breaking changes

# v2.0.0
**Breaking changes:**

### HttpClientFactory
The constructor argument of `HttpClientFactory` was changed from `Uri` to `HttpClient`. The `IHttpClientFactory` is registered with `System.Net.Http.AddHttpClient` for better lifecycle management of the resources used by `HttpClients`.
`IHttpClientFactory` in this case should be probably renamed to `IHttpClientProvider` or something similar, but I decided to avoid such a "big"
 breaking change. In case of too much confusion please let me know.

This design decision was made based on the eBook ".NET Microservices Architecture for Containerized .NET Applications" to be able to take advantage of the AddHttpClient extension method. 

> Though this class (HtppClient) implements IDisposable, declaring and instantiating it within a using statement is not preferred because when the HttpClient object gets disposed of, the underlying socket is not immediately released, which can lead to a socket exhaustion problem.

Therefore, HttpClient is intended to be [instantiated once and reused](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net) throughout the life of an application. `KSqlDbServiceCollectionExtensions.AddDbContext<>` internally registers `IHttpClientFactory` in the following manner:
```C#
internal static IServiceCollection ConfigureHttpClients(this IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
{
  //...

  serviceCollection.AddHttpClient<IHttpClientFactory, HttpClientFactory>(httpClient =>
  {
    httpClient.DefaultRequestHeaders.Add("ApiKey", "admin123");
  });
}
```

### Package references
- upgraded package references `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging.Abstractions` to v6.0.0
- added package reference `Microsoft.Extensions.Http` v6.0.0

# **Breaking changes.**
In order to improve the v1.0.0 release the following methods and properties were renamed:

IKSqlDbRestApiClient interface changes:

| v0.11.0                 | v1.0.0                       |
|-------------------------|------------------------------|
| `CreateTable`           | `CreateTableAsync`           |
| `CreateStream`          | `CreateStreamAsync`          |
| `CreateOrReplaceTable`  | `CreateOrReplaceTableAsync`  |
| `CreateOrReplaceStream` | `CreateOrReplaceStreamAsync` |

KSQL documentation refers to stream or table name in FROM as [from_item](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/select-push-query/)

```
IKSqlDBContext.CreateQuery<TEntity>(string streamName = null)
IKSqlDBContext.CreateQueryStream<TEntity>(string streamName = null)
```
streamName parameters were renamed to fromItemName:
```
IKSqlDBContext.CreateQuery<TEntity>(string fromItemName = null)
IKSqlDBContext.CreateQueryStream<TEntity>(string fromItemName = null)
```
```
QueryContext.StreamName property was renamed to QueryContext.FromItemName
Source.Of parameter streamName was renamed to fromItemName
KSqlDBContextOptions.ShouldPluralizeStreamName was renamed to ShouldPluralizeFromItemName
```

Record.RowTime was decorated with IgnoreByInsertsAttribute

> ⚠  From version 1.0.0 the overridden from item names are pluralized, too. 
Join items are also affected by this breaking change. This breaking change can cause runtime exceptions for users updating from lower versions. In case that you have never used custom singular from-item names, your code won't be affected, see the example below:

```
var contextOptions = new KSqlDBContextOptions(@"http:\\localhost:8088")
{
  //Default value:  
  //ShouldPluralizeFromItemName = true
};

var query = new KSqlDBContext(contextOptions)
  .CreateQueryStream<Tweet>("Tweet")
  .ToQueryString();
```

KSQL generated since v1.0

```KSQL
SELECT * FROM Tweets EMIT CHANGES;
```

KSQL generated before v1.0

```KSQL
SELECT * FROM Tweet EMIT CHANGES;
```
