using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.Tests.Helpers;
using Moq;
using Moq.Protected;

namespace Kafka.DotNet.ksqlDB.Tests.Fakes.Http
{
  public static class FakeHttpClient
  {
    public static HttpClient CreateWithResponse(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {     
      var handlerMock = new Mock<HttpMessageHandler>();

      handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          nameof(HttpClient.SendAsync),
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage()
        {
          StatusCode = statusCode,
          Content = new StringContent(responseContent),
        })
        .Verifiable();

      return new HttpClient(handlerMock.Object)
      {
        BaseAddress = new Uri(TestParameters.KsqlDBUrl)
      };
    }
  }
}