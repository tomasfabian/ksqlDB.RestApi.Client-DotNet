using System.Collections.Generic;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Parameters
{
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
}