using System.Net;
using ksqlDb.RestApi.Client.ProtoBuf.Tests.Helpers;
using Moq;
using Moq.Protected;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.RestApi;

public static class FakeHttpClient
{
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
      BaseAddress = new Uri(TestParameters.KsqlDbUrl)
    };
  }

  public static HttpClient CreateWithResponse(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
  {
    var handlerMock = CreateHttpMessageHandler(responseContent, statusCode);

    return handlerMock.ToHttpClient();
  }
}