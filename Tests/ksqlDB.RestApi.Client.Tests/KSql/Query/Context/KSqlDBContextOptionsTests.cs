using System.Collections.Generic;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Context
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
    public void NotSetBasicAuthCredentials()
    {
      //Arrange

      //Act

      //Assert
      ClassUnderTest.UseBasicAuth.Should().BeFalse();
      ClassUnderTest.BasicAuthUserName.Should().BeEmpty();
      ClassUnderTest.BasicAuthPassword.Should().BeEmpty();
    }
    
    [TestMethod]
    public void SetBasicAuthCredentials()
    {
      //Arrange
      string userName = "fred";
      string password = "letmein";

      //Act
      ClassUnderTest
        .SetBasicAuthCredentials(userName, password);

      //Assert
      ClassUnderTest.UseBasicAuth.Should().BeTrue();
      ClassUnderTest.BasicAuthUserName.Should().Be(userName);
      ClassUnderTest.BasicAuthPassword.Should().Be(password);
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