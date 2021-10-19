using System;
using System.Net.Http;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http
{
  internal class HttpClientFactoryWithBasicAuth : IHttpClientFactory
  {
    private readonly Uri uri;
    private readonly BasicAuthCredentials basicAuthCredentials;

    public HttpClientFactoryWithBasicAuth(Uri uri, BasicAuthCredentials basicAuthCredentials)
    {
      this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
      this.basicAuthCredentials = basicAuthCredentials ?? throw new ArgumentNullException(nameof(basicAuthCredentials));
    }

    public HttpClient CreateClient()
    {
      return new HttpClient(new BasicAuthHandler(basicAuthCredentials))
      {
        BaseAddress = uri
      };
    }
  }
}