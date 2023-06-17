# Config

### KSqlDbContextOptionsBuilder.ReplaceHttpClient
If you want to use your own or third-party `HttpMessageHandlers`, you can achieve this by following the example below:
```C#
using System;
using System.Threading.Tasks;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.DependencyInjection;

private static void Configure(this IServiceCollection serviceCollection, string ksqlDbUrl)
{
  serviceCollection.AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
  {
    c.UseKSqlDb(ksqlDbUrl);

    c.ReplaceHttpClient<IHttpClientFactory, HttpClientFactory>(_ => {})
     .ConfigurePrimaryHttpMessageHandler(sp =>
     {
       X509Certificate2 clientCertificate = CreateClientCertificate();

       var httpClientHandler = new HttpClientHandler
       {
         ClientCertificateOptions = ClientCertificateOption.Manual
       };

       httpClientHandler.ClientCertificates.Add(clientCertificate);

       return httpClientHandler;
     })
     .AddHttpMessageHandler(_ => new DebugHandler());
  });
}

internal class DebugHandler : System.Net.Http.DelegatingHandler
{
  protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(
    System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
  {
    System.Diagnostics.Debug.WriteLine($"Process request: {request.RequestUri}");

    return base.SendAsync(request, cancellationToken);
  }
}
```

### Register ksqlDB dependencies

During application startup, the services required by the `IKSqlDBContext` and `IKSqlDbRestApiClient` can be registered for **dependency injection**. This allows components that need these services to receive them through constructor parameters.

```C#
using ksqlDb.RestApi.Client.DependencyInjection;

serviceCollection.ConfigureKSqlDb(ksqlDbUrl);
```

The `ConfigureKSqlDb` extension method, as demonstrated below, takes care of registering ksqldDB-related services in the **dependency injection container** on your behalf:

```C#
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;

var ksqlDbUrl = @"http://localhost:8088";

var serviceCollection = new ServiceCollection();

serviceCollection.AddHttpClient<ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory, ksqlDB.RestApi.Client.KSql.RestApi.Http.HttpClientFactory>(httpClient =>
{
  httpClient.BaseAddress = new Uri(ksqlDbUrl);
});

serviceCollection.AddScoped<IKSqlDbRestApiClient, KSqlDbRestApiClient>();

var provider = serviceCollection.BuildServiceProvider();

var restApiClient = provider.GetRequiredService<IKSqlDbRestApiClient>();
```

### Bearer token authentication

In your client application, you can include a **Bearer token** in the request headers when interacting with the `ksqlDB` server. This can typically be done by adding an "Authorization" header with the value `"Bearer <token>"`.

```C#
using System.Net.Http.Headers;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.DependencyInjection;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.Api.Client.Samples;

public static class KSqlDDbServiceCollectionExtensions
{
  public static void Configure(this IServiceCollection services, string ksqlDbUrl)
  {
    services.AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
    {
      c.UseKSqlDb(ksqlDbUrl);

      c.ReplaceHttpClient<IHttpClientFactory, HttpClientFactory>(_ => { })
        .AddHttpMessageHandler(_ => new BearerAuthHandler());
    });
  }
}

internal class BearerAuthHandler : DelegatingHandler
{
  public BearerAuthHandler()
  {
    InnerHandler = new HttpClientHandler();
  }

  protected override Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
  {
    var token = "xoidiag"; //CreateToken();

    request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

    return base.SendAsync(request, cancellationToken);
  }
}

```

### DisposeHttpClient
In .NET, it's important to properly **dispose** of `HttpClient` instances to release underlying resources and avoid potential issues with resource exhaustion.

`KSqlDBContextOptions` and `KSqlDbRestApiClient` - `DisposeHttpClient` property is by default set to `false`. From v2.0.0 the used `HttpClients` will not be disposed by default.

It is possible to override the aforementioned behavior in the following ways, although it is not recommended:
```C#
var contextOptions = new KSqlDBContextOptions(ksqlDbUrl)
{
  DisposeHttpClient = true
};
```

```C#
var httpClient = new HttpClient
{
  BaseAddress = new Uri(ksqlDbUrl)
};

var httpClientFactory = new HttpClientFactory(httpClient);
var kSqlDbRestApiClient = new KSqlDbRestApiClient(httpClientFactory)
{
  DisposeHttpClient = true
};
```

The recommended approach is to create a single instance of `HttpClient` and reuse it throughout the lifespan of an application.

To obtain an instance of `HttpClient` using `IHttpClientFactory` from the `ServicesCollection` in .NET for `IKSqlDbRestApiClient` and `IKSqlDBContext`, you can follow the steps in this [section](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/ksqldbcontext.md#ksqldbservicecollectionextensions---adddbcontext-and-adddbcontextfactory).

### SetJsonSerializerOptions
**v1.4.0**

- KSqlDbContextOptionsBuilder and KSqlDbContextOptions `SetJsonSerializerOptions` - a way to configure the JsonSerializerOptions for the materialization of the incoming values.

With `System.Text.Json` **source generators**, you can automatically generate serialization and deserialization code for **JSON models**, eliminating the need for manual code writing and reducing boilerplate code. This feature improves performance and reduces maintenance efforts when working with JSON data.

For better performance you can use the new `System.Text.Json` **source generator** in this way:

```C#
var contextOptions = new KSqlDbContextOptionsBuilder()
        .UseKSqlDb(ksqlDbUrl)
        .SetJsonSerializerOptions(c =>
        {
          c.Converters.Add(new CustomJsonConverter());

          jsonOptions.AddContext<SourceGenerationContext>();
        }).Options;

//or
contextOptions = new KSqlDBContextOptions(ksqlDbUrl)
  .SetJsonSerializerOptions(serializerOptions =>
                            {
                              serializerOptions.Converters.Add(new CustomJsonConverter());

                              jsonOptions.AddContext<SourceGenerationContext>();
                            });
```

```C#
using System.Text.Json.Serialization;
using ksqlDB.Api.Client.Samples.Models.Movies;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Movie))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
```

### ProcessingGuarantee enum
**v1.0.0**

In `ksqlDB`, **processing guarantees** refer to the level of reliability and consistency provided by the system when processing and handling streaming data.

**ExactlyOnce** - Records are processed once. To achieve a true exactly-once system, end consumers and producers must also implement exactly-once semantics.

**AtLeastOnce** - Records are never lost but may be redelivered.

For more info check [exactly once semantics](https://docs.ksqldb.io/en/latest/operate-and-deploy/exactly-once-semantics/)

```C#
public enum ProcessingGuarantee
{
  ExactlyOnce,
  ExactlyOnceV2,
  AtLeastOnce
}
```
