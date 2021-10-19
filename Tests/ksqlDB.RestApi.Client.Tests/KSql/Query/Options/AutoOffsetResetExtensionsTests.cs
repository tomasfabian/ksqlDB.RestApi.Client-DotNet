using System;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Options
{
  [TestClass]
  public class AutoOffsetResetExtensionsTests
  {
    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ToAutoOffsetReset_UnknownValue()
    {
      //Arrange

      //Act
      "xyz".ToAutoOffsetReset();

      //Assert
    }

    [TestMethod]
    public void ToAutoOffsetReset_Earliest()
    {
      //Arrange

      //Act
      var value = "earliest".ToAutoOffsetReset();

      //Assert
      value.Should().Be(AutoOffsetReset.Earliest);
    }
    
    [TestMethod]
    public void ToAutoOffsetReset_Latest()
    {
      //Arrange

      //Act
      var value = "latest".ToAutoOffsetReset();

      //Assert
      value.Should().Be(AutoOffsetReset.Latest);
    }
    
    [TestMethod]
    public void ToKSqlValue_Earliest()
    {
      //Arrange

      //Act
      var value = AutoOffsetReset.Earliest.ToKSqlValue();

      //Assert
      value.Should().BeEquivalentTo("earliest");
    }
    
    [TestMethod]
    public void ToKSqlValue_Latest()
    {
      //Arrange

      //Act
      var value = AutoOffsetReset.Latest.ToKSqlValue();

      //Assert
      value.Should().BeEquivalentTo("latest");
    }
  }
}