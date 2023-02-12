# Breaking changes

# v3.0.0
- property AsyncDisposableObject.IsDisposed was changed from public to an internal access modifier

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
  });
}
```

### Package references
- upgraded package references `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging.Abstractions` to v6.0.0
- added package reference `Microsoft.Extensions.Http` v6.0.0
