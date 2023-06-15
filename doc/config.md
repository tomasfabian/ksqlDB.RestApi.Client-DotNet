# Config

### KSqlDbContextOptionsBuilder.ReplaceHttpClient
In cases when you would like to provide your own or 3rd party `HttpMessageHandlers` you can do it like in the bellow example:

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

### Register KSqlDB dependencies

```C#
using ksqlDb.RestApi.Client.DependencyInjection;

serviceCollection.ConfigureKSqlDb(ksqlDbUrl);
```

The above used extension method `ConfigureKSqlDb` registers services behalf of you as illustrated bellow:

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
`KSqlDBContextOptions` and `KSqlDbRestApiClient` - `DisposeHttpClient` property is by default set to `false`. From v2.0.0 the used `HttpClients` will not be disposed by default.

The above mentioned behavior can be overridden in the following ways:
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

### SetJsonSerializerOptions
**v1.4.0**

- KSqlDbContextOptionsBuilder and KSqlDbContextOptions SetJsonSerializerOptions - a way to configure the JsonSerializerOptions for the materialization of the incoming values.

For better performance you can use the new `System.Text.Json` source generator:

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

**ExactlyOnce** - Records are processed once. To achieve a true exactly-once system, end consumers and producers must also implement exactly-once semantics.
**AtLeastOnce** - Records are never lost but may be redelivered.

For more info check [exactly once semantics](https://docs.ksqldb.io/en/latest/operate-and-deploy/exactly-once-semantics/)

```C#
public enum ProcessingGuarantee
{
  ExactlyOnce,
  AtLeastOnce
}
```
