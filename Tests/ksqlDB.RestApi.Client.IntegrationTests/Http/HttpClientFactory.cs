using System;
using System.Net.Http;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.Api.Client.IntegrationTests.Http;

public class HttpClientFactory : IHttpClientFactory
{
  private readonly Uri uri;

  public HttpClientFactory(Uri uri)
  {
    this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
  }

  public HttpClient CreateClient()
  {
    return new()
    {
      BaseAddress = uri
    };
  }
}