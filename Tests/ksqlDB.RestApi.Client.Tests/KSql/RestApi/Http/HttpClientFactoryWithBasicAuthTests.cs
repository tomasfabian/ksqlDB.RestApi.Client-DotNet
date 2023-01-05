using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Http;

[TestClass]
public class HttpClientFactoryWithBasicAuthTests : TestBase
{
  [TestMethod]
  public void CreateClient_BaseAddressWasSet()
  {
    //Arrange
    var credentials = new BasicAuthCredentials
    {
      UserName = "fred",
      Password = "letmein"
    };

    var httpClientFactory = new HttpClientFactoryWithBasicAuth(new Uri(TestParameters.KsqlDBUrl), credentials);

    //Act
    var httpClient = httpClientFactory.CreateClient();

    //Assert
    httpClient.Should().BeOfType<HttpClient>();
    httpClient.BaseAddress!.OriginalString.Should().BeEquivalentTo(TestParameters.KsqlDBUrl);
  }
}
