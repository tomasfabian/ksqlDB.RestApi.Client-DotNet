using System;
using System.Net.Http;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http;

public class HttpClientFactory : IHttpV1ClientFactory
{
  private readonly HttpClient httpClient;

  public HttpClientFactory(HttpClient httpClient)
  {
    this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
  }

  public HttpClient CreateClient()
  {
    return httpClient;
  }
}