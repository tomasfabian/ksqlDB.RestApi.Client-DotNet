using ksqlDb.RestApi.Client.Tests.Fakes.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi;

public abstract class KSqlDbRestApiClientTestsBase : TestBase
{
  protected IHttpClientFactory HttpClientFactory = null!;
  protected HttpClient HttpClient = null!;
  protected Mock<HttpMessageHandler> HttpMessageHandlerMock = null!;

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    HttpClientFactory = Mock.Of<IHttpClientFactory>();
  }

  protected void CreateHttpMocks(string responseContents)
  {
    HttpMessageHandlerMock = FakeHttpClient.CreateHttpMessageHandler(responseContents);

    HttpClient = HttpMessageHandlerMock.ToHttpClient();

    Mock.Get(HttpClientFactory).Setup(c => c.CreateClient()).Returns(HttpClient);
  }
}
