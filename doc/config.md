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
var kSqlDbRestApiClient = new KSqlDbRestApiClient(new HttpClientFactory(new Uri(ksqlDbUrl)))
{
  DisposeHttpClient = true
};
```

