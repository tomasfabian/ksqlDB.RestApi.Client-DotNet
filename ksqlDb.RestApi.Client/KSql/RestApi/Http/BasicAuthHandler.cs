using System.Net.Http.Headers;
using ksqlDB.RestApi.Client.KSql.Query.Context;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http;

internal class BasicAuthHandler : DelegatingHandler
{
  private readonly KSqlDBContextOptions options;

  public BasicAuthHandler(KSqlDBContextOptions options)
  {
    this.options = options ?? throw new ArgumentNullException(nameof(options));
  }

  protected override Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken
  )
  {
    var token = new BasicAuthCredentials(
      options.BasicAuthUserName,
      options.BasicAuthPassword
    ).CreateToken();

    request.Headers.Authorization = new AuthenticationHeaderValue(
      BasicAuthCredentials.Schema,
      token
    );

    return base.SendAsync(request, cancellationToken);
  }
}
