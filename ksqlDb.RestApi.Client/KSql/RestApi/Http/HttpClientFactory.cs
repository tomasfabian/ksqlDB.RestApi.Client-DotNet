using System;
using System.Net.Http;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http
{
  public class HttpClientFactory : IHttpClientFactory
  {
    private readonly HttpClient httpClient;

    public HttpClientFactory(Uri uri)
    {
      if(uri == null)
        throw new ArgumentNullException(nameof(uri));

      httpClient = new()
      {
        BaseAddress = uri
      };
    }

    public HttpClient CreateClient()
    {
      return httpClient;
    }
  }
}