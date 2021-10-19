using FluentAssertions;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.Infrastructure.Extensions
{
  [TestClass]
  public class StringExtensionsTests
  {
    [TestMethod]
    public void ToKSqlFunctionName()
    {
      //Arrange
      string functionName = "ExtractJsonField";

      //Act
      var ksqlFunctionName = functionName.ToKSqlFunctionName();

      //Assert
      ksqlFunctionName.Should().Be("EXTRACT_JSON_FIELD");
    }

    [TestMethod]
    public void IsNotNullOrEmpty()
    {
      //Arrange

      //Act
      var isNotNullOrEmpty = "".IsNotNullOrEmpty();

      //Assert
      isNotNullOrEmpty.Should().BeFalse();
    }

    [TestMethod]
    public void IsNotNullOrEmpty_TextString_ReturnsTrue()
    {
      //Arrange

      //Act
      var isNotNullOrEmpty = "ksql".IsNotNullOrEmpty();

      //Assert
      isNotNullOrEmpty.Should().BeTrue();
    }
  }
}