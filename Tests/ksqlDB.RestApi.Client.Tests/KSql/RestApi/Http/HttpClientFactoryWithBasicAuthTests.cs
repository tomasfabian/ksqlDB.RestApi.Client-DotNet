using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using NUnit.Framework;
using UnitTests;
using TestParameters = ksqlDB.Api.Client.Tests.Helpers.TestParameters;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Http;

public class HttpClientFactoryWithBasicAuthTests : TestBase
{
  [Test]
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
