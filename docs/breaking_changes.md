# Breaking changes

# v6.0.0
- `QueryType` enum was renamed to `EndpointType`
- removed `CreateQuery` from `IKSqlDBContext` interface in the netstandard version. It was replaced with `CreateQueryStream`.
 
# v5.0.0
- removed static modifier from `StatementGenerator` class

# v4.0.0
- `TypeProperties` - removed generic type argument and added a base type `EntityProperties`. Additionally, changed from a class to a record type.
- the above change affected the argument for the `IKSqlDbRestApiClient.CreateTypeAsync<T>(TypeProperties typeProperties)`
- `IEntityCreationProperties` was replaced with `IEntityProperties`
- `ksqlDB` types are no longer **automatically capitalized** in `CREATE TYPE` statements
- removed parameterless constructor for `EntityCreationMetadata`. The Kafka topic is a mandatory field.
- enabled nullable reference types in `ksqlDb.RestApi.Client` project

Updated package references:
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
<PackageReference Include="System.Text.Json" Version="8.0.2" />
```

# v3.0.0
- The **accessibility modifier** of the `IsDisposed` property in `AsyncDisposableObject` was modified from public to internal.

Removed not supported TFMs:
- netcoreapp3.1;net5.0

Removed obsolete methods:
- `KSqlFunctionsExtensions.Sign`
- `IAggregations<TSource>.CollectList`

Upgraded .NET package dependencies:

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="6.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="6.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="6.0.0" />
<PackageReference Include="System.Interactive.Async" Version="5.0.0" />
<PackageReference Include="System.Text.Json" Version="5.0.0" />
```

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="7.0.0" />
<PackageReference Include="System.Interactive.Async" Version="6.0.1" />
<PackageReference Include="System.Text.Json" Version="7.0.0" />
```

# v2.0.0
**Breaking changes:**

### HttpClientFactory

The constructor parameter in `HttpClientFactory` was updated from 'Uri' to 'HttpClient' to improve the management of resources used by `HttpClient`s. The `IHttpClientFactory` is registered using `System.Net.Http.AddHttpClient` to facilitate better lifecycle management.
Although it may be appropriate to rename `IHttpClientFactory` to `IHttpClientProvider` or a similar name, I opted not to make such a significant breaking change. If this decision causes significant confusion, please inform me.

The decision to make this design change was influenced by the eBook ".NET Microservices Architecture for Containerized .NET Applications" in order to leverage the benefits offered by the `AddHttpClient` extension method:

> Though this class (HtppClient) implements IDisposable, declaring and instantiating it within a using statement is not preferred because when the HttpClient object gets disposed of, the underlying socket is not immediately released, which can lead to a socket exhaustion problem.

Therefore, HttpClient is intended to be [instantiated once and reused](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net) throughout the life of an application. `KSqlDbServiceCollectionExtensions.AddDbContext<>` internally registers `IHttpClientFactory` in the following manner:
```C#
internal static IServiceCollection ConfigureHttpClients(this IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
{
  //...

  serviceCollection.AddHttpClient<IHttpClientFactory, HttpClientFactory>(httpClient =>
  {
  });
}
```

### Package references
- upgraded package references `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging.Abstractions` to v6.0.0
- added package reference `Microsoft.Extensions.Http` v6.0.0
