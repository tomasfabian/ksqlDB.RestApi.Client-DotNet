using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using NUnit.Framework;
using UnitTests;
using TestParameters = ksqlDB.Api.Client.Tests.Helpers.TestParameters;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Http;

public class HttpClientFactoryTests : TestBase
{
  [Test]
  public void CreateClient_BaseAddressWasSet()
  {
    //Arrange
    var httpClient = new HttpClient()
    {
      BaseAddress = new Uri(TestParameters.KsqlDBUrl)
    };

    var httpClientFactory = new HttpClientFactory(httpClient);

    //Act
    var receivedHttpClient = httpClientFactory.CreateClient();

    //Assert
    receivedHttpClient.Should().BeSameAs(httpClient);
    receivedHttpClient.Should().BeOfType<HttpClient>();
    receivedHttpClient.BaseAddress!.OriginalString.Should().BeEquivalentTo(TestParameters.KsqlDBUrl);
  }
}
