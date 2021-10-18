using System;
using System.Net.Http;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Http;
using ksqlDB.Api.Client.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Http
{
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
      httpClient.BaseAddress.OriginalString.Should().BeEquivalentTo(TestParameters.KsqlDBUrl);
    }
  }
}