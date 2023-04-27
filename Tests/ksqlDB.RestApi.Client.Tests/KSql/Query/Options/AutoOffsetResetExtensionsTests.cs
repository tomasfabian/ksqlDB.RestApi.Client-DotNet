using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Options;

public class AutoOffsetResetExtensionsTests
{
  [Test]
  public void ToAutoOffsetReset_UnknownValue()
  {
    //Arrange

    //Assert
    Assert.Throws<ArgumentOutOfRangeException>(() =>
    {
      //Act
      "xyz".ToAutoOffsetReset();
    });
  }

  [Test]
  public void ToAutoOffsetReset_Earliest()
  {
    //Arrange

    //Act
    var value = "earliest".ToAutoOffsetReset();

    //Assert
    value.Should().Be(AutoOffsetReset.Earliest);
  }
    
  [Test]
  public void ToAutoOffsetReset_Latest()
  {
    //Arrange

    //Act
    var value = "latest".ToAutoOffsetReset();

    //Assert
    value.Should().Be(AutoOffsetReset.Latest);
  }
    
  [Test]
  public void ToKSqlValue_Earliest()
  {
    //Arrange

    //Act
    var value = AutoOffsetReset.Earliest.ToKSqlValue();

    //Assert
    value.Should().BeEquivalentTo("earliest");
  }
    
  [Test]
  public void ToKSqlValue_Latest()
  {
    //Arrange

    //Act
    var value = AutoOffsetReset.Latest.ToKSqlValue();

    //Assert
    value.Should().BeEquivalentTo("latest");
  }
}
