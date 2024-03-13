using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using NUnit.Framework;
using UnitTests;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Http;

public class HttpClientFactoryWithBasicAuthTests : TestBase
{
  [Test]
  public void CreateClient_BaseAddressWasSet()
  {
    //Arrange
    var credentials = new BasicAuthCredentials("fred", "letmein");

    var httpClientFactory = new HttpClientFactoryWithBasicAuth(new Uri(TestParameters.KsqlDbUrl), credentials);

    //Act
    var httpClient = httpClientFactory.CreateClient();

    //Assert
    httpClient.Should().BeOfType<HttpClient>();
    httpClient.BaseAddress!.OriginalString.Should().BeEquivalentTo(TestParameters.KsqlDbUrl);
  }
}
