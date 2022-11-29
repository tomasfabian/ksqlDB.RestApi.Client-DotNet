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
