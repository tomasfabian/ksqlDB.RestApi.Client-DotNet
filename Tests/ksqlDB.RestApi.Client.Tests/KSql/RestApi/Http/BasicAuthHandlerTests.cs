using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Http;

[TestClass]
public class BasicAuthHandlerTests : TestBase
{
  [TestMethod]
  public async Task SendAsync()
  {
    //Arrange
    var credentials = new BasicAuthCredentials
    {
      UserName = "fred",
      Password = "letmein"
    };

    var handler = new BasicAuthHandler(credentials);
    handler.InnerHandler = new HttpClientHandler();
    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://tests.com/");
    var invoker = new HttpMessageInvoker(handler);

    //Act
    var result = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

    //Assert
    httpRequestMessage.Headers.Authorization.Scheme.Should().Be("basic");
    httpRequestMessage.Headers.Authorization.Parameter.Should().Be("ZnJlZDpsZXRtZWlu");
  }
}