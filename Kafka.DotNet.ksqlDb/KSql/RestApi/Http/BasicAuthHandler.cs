using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Http
{
  internal class BasicAuthHandler : DelegatingHandler
  {
    private readonly BasicAuthCredentials basicAuthCredentials;

    public BasicAuthHandler(BasicAuthCredentials basicAuthCredentials)
    {
      this.basicAuthCredentials = basicAuthCredentials ?? throw new ArgumentNullException(nameof(basicAuthCredentials));

      InnerHandler = new HttpClientHandler();
    }
	
    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
      var token = basicAuthCredentials.CreateToken();

      request.Headers.Authorization = new AuthenticationHeaderValue(basicAuthCredentials.Schema, token);

      return base.SendAsync(request, cancellationToken);
    }
  }
}