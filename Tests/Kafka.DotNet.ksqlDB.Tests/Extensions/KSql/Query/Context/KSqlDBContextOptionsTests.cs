﻿using System.Collections.Generic;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Config;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Kafka.DotNet.ksqlDB.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Context
{
  [TestClass]
  public class KSqlDBContextOptionsTests : TestBase<KSqlDBContextOptions>
  {
    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
    }

    [TestMethod]
    public void Url_ShouldNotBeEmpty()
    {
      //Arrange

      //Act
      var url = ClassUnderTest.Url;

      //Assert
      url.Should().Be(TestParameters.KsqlDBUrl);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void SetProcessingGuarantee_WasNotSet()
    {
      //Arrange
      string parameterName = KSqlDbConfigs.ProcessingGuarantee;

      //Act

      //Assert
      ClassUnderTest.QueryParameters[parameterName].Should().BeEmpty();
    }

    [TestMethod]
    public void SetProcessingGuarantee_SetToAtLeastOnce()
    {
      //Arrange
      var processingGuarantee = ProcessingGuarantee.AtLeastOnce;
      string parameterName = KSqlDbConfigs.ProcessingGuarantee;

      //Act
      ClassUnderTest.SetProcessingGuarantee(processingGuarantee);

      //Assert
      string expectedValue = "at_least_once";

      ClassUnderTest.QueryParameters[parameterName].Should().Be(expectedValue);
      ClassUnderTest.QueryStreamParameters[parameterName].Should().Be(expectedValue);
    }

    [TestMethod]
    public void SetProcessingGuarantee_SetToExactlyOnce()
    {
      //Arrange
      var processingGuarantee = ProcessingGuarantee.ExactlyOnce;
      string parameterName = KSqlDbConfigs.ProcessingGuarantee;

      //Act
      ClassUnderTest.SetProcessingGuarantee(processingGuarantee);

      //Assert
      string expectedValue = "exactly_once";

      ClassUnderTest.QueryParameters[parameterName].Should().Be(expectedValue);
      ClassUnderTest.QueryStreamParameters[parameterName].Should().Be(expectedValue);
    }

    [TestMethod]
    public void SetAutoOffsetReset()
    {
      //Arrange
      var autoOffsetReset = AutoOffsetReset.Latest;

      //Act
      ClassUnderTest.SetAutoOffsetReset(autoOffsetReset);

      //Assert
      string expectedValue = autoOffsetReset.ToString().ToLower();

      ClassUnderTest.QueryParameters[QueryParameters.AutoOffsetResetPropertyName].Should().Be(expectedValue);
      ClassUnderTest.QueryStreamParameters[QueryStreamParameters.AutoOffsetResetPropertyName].Should().Be(expectedValue);
    }
    
    [TestMethod]
    public void Clone()
    {
      //Arrange
      var processingGuarantee = ProcessingGuarantee.AtLeastOnce;
      string parameterName = KSqlDbConfigs.ProcessingGuarantee;
      ClassUnderTest.SetProcessingGuarantee(processingGuarantee);

      //Act
      var clone = ClassUnderTest.Clone();

      //Assert
      string expectedValue = "at_least_once";
      
      ClassUnderTest.Url.Should().Be(TestParameters.KsqlDBUrl);

      clone.QueryParameters[parameterName].Should().Be(expectedValue);
      clone.QueryStreamParameters[parameterName].Should().Be(expectedValue);
    }
  }
}