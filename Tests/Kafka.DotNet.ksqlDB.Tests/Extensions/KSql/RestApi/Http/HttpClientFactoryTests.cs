using System;
using System.Net.Http;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Http
{
  [TestClass]
  public class HttpClientFactoryTests : TestBase
  {
    [TestMethod]
    public void CreateClient_BaseAddressWasSet()
    {
      //Arrange
      var httpClientFactory = new HttpClientFactory(new Uri(TestParameters.KsqlDBUrl));

      //Act
      var httpClient = httpClientFactory.CreateClient();

      //Assert
      httpClient.Should().BeOfType<HttpClient>();
      httpClient.BaseAddress.OriginalString.Should().BeEquivalentTo(TestParameters.KsqlDBUrl);
    }
  }
}