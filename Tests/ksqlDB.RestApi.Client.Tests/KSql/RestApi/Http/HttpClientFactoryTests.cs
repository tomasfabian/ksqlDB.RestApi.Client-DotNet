using System;
using System.Net.Http;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Http
{
  [TestClass]
  public class HttpClientFactoryTests : TestBase
  {
    [TestMethod]
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
}