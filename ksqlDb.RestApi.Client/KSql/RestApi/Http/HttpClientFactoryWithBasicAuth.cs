using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http;

internal class HttpClientFactoryWithBasicAuth : IHttpClientFactory
{
  private readonly Uri uri;
  private readonly KSqlDBContextOptions options;

  public HttpClientFactoryWithBasicAuth(Uri uri, KSqlDBContextOptions options)
  {
    this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
    this.options = options ?? throw new ArgumentNullException(nameof(options));
  }

  public HttpClient CreateClient()
  {
    return new HttpClient(new BasicAuthHandler(options)) { BaseAddress = uri };
  }
}
