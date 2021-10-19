using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Http
{
  [TestClass]
  public class BasicAuthCredentialsTests : TestBase
  {
    [TestMethod]
    public void CreateToken_Ctor()
    {
      //Arrange
      var credentials = new BasicAuthCredentials("fred", "letmein");

      //Act
      var token = credentials.CreateToken();

      //Assert
      token.Should().Be(expectedToken);
    }

    private readonly string expectedToken = "ZnJlZDpsZXRtZWlu";

    [TestMethod]
    public void CreateToken_FromProperties()
    {
      //Arrange
      var credentials = new BasicAuthCredentials
      {
        UserName = "fred",
        Password = "letmein"
      };

      //Act
      var token = credentials.CreateToken();

      //Assert
      token.Should().Be(expectedToken);
    }

    [TestMethod]
    public void Schema_Basic()
    {
      //Arrange
      var credentials = new BasicAuthCredentials
      {
        UserName = "fred",
        Password = "letmein"
      };

      //Act
      var token = credentials.Schema;

      //Assert
      token.Should().Be("basic");
    }
  }
}