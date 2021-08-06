using System.Net.Http;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.Tests.Fakes.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi
{
  public abstract class KSqlDbRestApiClientTestsBase : TestBase
  {
    protected IHttpClientFactory HttpClientFactory;
    protected HttpClient HttpClient;
    protected Mock<HttpMessageHandler> httpMessageHandlerMock;

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      HttpClientFactory = Mock.Of<IHttpClientFactory>();
    }

    protected void CreateHttpMocks(string responseContents)
    {
      httpMessageHandlerMock = FakeHttpClient.CreateHttpMessageHandler(responseContents);

      HttpClient = httpMessageHandlerMock.ToHttpClient();

      Mock.Get(HttpClientFactory).Setup(c => c.CreateClient()).Returns(HttpClient);
    }
  }
}