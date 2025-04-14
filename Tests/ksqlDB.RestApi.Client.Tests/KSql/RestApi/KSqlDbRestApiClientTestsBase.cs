using ksqlDb.RestApi.Client.Tests.Fakes.Http;
using Moq;
using NUnit.Framework;
using UnitTests;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi;

public abstract class KSqlDbRestApiClientTestsBase : TestBase
{
  protected IHttpClientFactory HttpClientFactory = null!;
  protected HttpClient HttpClient = null!;
  protected Mock<HttpMessageHandler> HttpMessageHandlerMock = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    HttpClientFactory = Mock.Of<IHttpClientFactory>();
  }

  [TearDown]
  public override void TestCleanup()
  {
    HttpClient?.Dispose();
    base.TestCleanup();
  }

  protected void CreateHttpMocks(string responseContents)
  {
    HttpMessageHandlerMock = FakeHttpClient.CreateHttpMessageHandler(responseContents);

    HttpClient = HttpMessageHandlerMock.ToHttpClient();

    Mock.Get(HttpClientFactory).Setup(c => c.CreateClient()).Returns(HttpClient);
  }
}
