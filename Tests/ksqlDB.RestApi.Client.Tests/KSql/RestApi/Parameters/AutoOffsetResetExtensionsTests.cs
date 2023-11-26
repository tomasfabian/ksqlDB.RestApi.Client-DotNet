using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parameters;

public class AutoOffsetResetExtensionsTests
{
  [Test]
  public void ToKSqlValue()
  {
    //Arrange

    //Act
    var result = AutoOffsetReset.Latest.ToKSqlValue();

    //Assert
    result.Should().Be("latest");
  }

  [Test]
  public void ToAutoOffsetReset()
  {
    //Arrange

    //Act
    var result = "latest".ToAutoOffsetReset();

    //Assert
    result.Should().Be(AutoOffsetReset.Latest);
  }
}
