using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnitTests;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Context;

public class KSqlDBContextOptionsTests : TestBase<KSqlDBContextOptions>
{
  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = new KSqlDBContextOptions(TestParameters.KsqlDbUrl);
  }

  [Test]
  public void Url_ShouldNotBeEmpty()
  {
    //Arrange

    //Act
    var url = ClassUnderTest.Url;

    //Assert
    url.Should().Be(TestParameters.KsqlDbUrl);
  }

  [Test]
  public void NotSetBasicAuthCredentials()
  {
    //Arrange

    //Act

    //Assert
    ClassUnderTest.UseBasicAuth.Should().BeFalse();
    ClassUnderTest.BasicAuthUserName.Should().BeEmpty();
    ClassUnderTest.BasicAuthPassword.Should().BeEmpty();
  }

  [Test]
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

  [Test]
  public void SetBasicAuthCredentialsDirectly()
  {
    //Arrange

    //Act
    ClassUnderTest.UseBasicAuth = true;

    //Assert
    ClassUnderTest.UseBasicAuth.Should().BeTrue();
    ClassUnderTest.BasicAuthUserName.Should().BeEmpty();
    ClassUnderTest.BasicAuthPassword.Should().BeEmpty();
  }

  [Test]
  public void SetProcessingGuarantee_WasNotSet()
  {
    //Arrange
    string parameterName = KSqlDbConfigs.ProcessingGuarantee;

    //Act

    //Assert
    Assert.ThrowsException<KeyNotFoundException>(() =>
      ClassUnderTest.QueryStreamParameters[parameterName].Should().BeEmpty());
  }

  [Test]
  public void SetProcessingGuarantee_SetToAtLeastOnce()
  {
    //Arrange
    var processingGuarantee = ProcessingGuarantee.AtLeastOnce;
    string parameterName = KSqlDbConfigs.ProcessingGuarantee;

    //Act
    ClassUnderTest.SetProcessingGuarantee(processingGuarantee);

    //Assert
    string expectedValue = "at_least_once";

    ClassUnderTest.QueryStreamParameters[parameterName].Should().Be(expectedValue);
  }

  [Test]
  public void SetProcessingGuarantee_SetToExactlyOnce()
  {
    //Arrange
    var processingGuarantee = ProcessingGuarantee.ExactlyOnce;
    string parameterName = KSqlDbConfigs.ProcessingGuarantee;

    //Act
    ClassUnderTest.SetProcessingGuarantee(processingGuarantee);

    //Assert
    string expectedValue = "exactly_once";

    ClassUnderTest.QueryStreamParameters[parameterName].Should().Be(expectedValue);
  }

  [Test]
  public void SetProcessingGuarantee_SetToExactlyOnceV2()
  {
    //Arrange
    var processingGuarantee = ProcessingGuarantee.ExactlyOnceV2;
    string parameterName = KSqlDbConfigs.ProcessingGuarantee;

    //Act
    ClassUnderTest.SetProcessingGuarantee(processingGuarantee);

    //Assert
    string expectedValue = "exactly_once_v2";

    ClassUnderTest.QueryStreamParameters[parameterName].Should().Be(expectedValue);
  }

  [Test]
  public void SetAutoOffsetReset()
  {
    //Arrange
    var autoOffsetReset = AutoOffsetReset.Latest;

    //Act
    ClassUnderTest.SetAutoOffsetReset(autoOffsetReset);

    //Assert
    string expectedValue = autoOffsetReset.ToString().ToLower();

    ClassUnderTest.QueryStreamParameters[QueryStreamParameters.AutoOffsetResetPropertyName].Should().Be(expectedValue);
  }

    
  [Test]
  public void JsonSerializerOptions()
  {
    //Arrange

    //Act
    var jsonSerializerOptions = ClassUnderTest.JsonSerializerOptions;

    //Assert
    jsonSerializerOptions.Should().NotBeNull();
  }
}
