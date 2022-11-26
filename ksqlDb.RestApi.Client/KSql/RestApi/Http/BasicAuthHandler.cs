using System.Net.Http.Headers;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http;

internal class BasicAuthHandler : DelegatingHandler
{
  private readonly BasicAuthCredentials basicAuthCredentials;

  public BasicAuthHandler(BasicAuthCredentials basicAuthCredentials)
  {
    this.basicAuthCredentials = basicAuthCredentials ?? throw new ArgumentNullException(nameof(basicAuthCredentials));
  }
	
  protected override Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
  {
    var token = basicAuthCredentials.CreateToken();

    request.Headers.Authorization = new AuthenticationHeaderValue(basicAuthCredentials.Schema, token);

    return base.SendAsync(request, cancellationToken);
  }
}