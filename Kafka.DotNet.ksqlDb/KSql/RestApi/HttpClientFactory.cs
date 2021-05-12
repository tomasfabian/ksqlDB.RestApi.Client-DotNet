using System;
using System.Net.Http;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{
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
}