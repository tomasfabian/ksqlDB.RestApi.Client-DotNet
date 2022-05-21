using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.Api.Client.Tests.Helpers;
using Moq;
using Moq.Protected;

namespace ksqlDB.Api.Client.Tests.Fakes.Http
{
  public static class FakeHttpClient
  {
    public static Mock<DelegatingHandler> CreateDelegatingHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      var handlerMock = new Mock<DelegatingHandler>();

      handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          nameof(HttpClient.SendAsync),
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = statusCode,
          Content = new StringContent(responseContent),
        })
        .Verifiable();

      return handlerMock;
    }

    public static Mock<HttpMessageHandler> CreateHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {     
      var handlerMock = new Mock<HttpMessageHandler>();

      handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          nameof(HttpClient.SendAsync),
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = statusCode,
          Content = new StringContent(responseContent),
        })
        .Verifiable();

      return handlerMock;
    }

    public static HttpClient ToHttpClient(this Mock<HttpMessageHandler> handlerMock)
    {     
      return new HttpClient(handlerMock.Object)
      {
        BaseAddress = new Uri(TestParameters.KsqlDBUrl)
      };
    }

    public static HttpClient CreateWithResponse(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {     
      var handlerMock = CreateHttpMessageHandler(responseContent, statusCode);

      return handlerMock.ToHttpClient();
    }
  }
}