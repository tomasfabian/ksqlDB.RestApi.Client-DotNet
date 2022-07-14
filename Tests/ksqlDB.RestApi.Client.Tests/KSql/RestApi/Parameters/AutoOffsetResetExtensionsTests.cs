using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Parameters;

[TestClass]
public class AutoOffsetResetExtensionsTests
{
  [TestMethod]
  public void ToKSqlValue()
  {
    //Arrange

    //Act
    var result = AutoOffsetReset.Latest.ToKSqlValue();

    //Assert
    result.Should().Be("latest");
  }

  [TestMethod]
  public void ToAutoOffsetReset()
  {
    //Arrange

    //Act
    var result = "latest".ToAutoOffsetReset();

    //Assert
    result.Should().Be(AutoOffsetReset.Latest);
  }
}