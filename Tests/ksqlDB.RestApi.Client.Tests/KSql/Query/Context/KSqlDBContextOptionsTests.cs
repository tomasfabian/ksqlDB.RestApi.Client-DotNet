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
  public void SetProcessingGuarantee_WasNotSet()
  {
    //Arrange
    string parameterName = KSqlDbConfigs.ProcessingGuarantee;

    //Act

    //Assert
    Assert.ThrowsException<KeyNotFoundException>(() =>
      ClassUnderTest.QueryStreamParameters.Get<ProcessingGuarantee>(parameterName));
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
    ClassUnderTest.QueryStreamParameters.Get<ProcessingGuarantee>(parameterName).Should().Be(processingGuarantee);
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
    ClassUnderTest.QueryStreamParameters.Get<ProcessingGuarantee>(parameterName).Should().Be(processingGuarantee);
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
    ClassUnderTest.QueryStreamParameters.Get<ProcessingGuarantee>(parameterName).Should().Be(processingGuarantee);
  }

  [Test]
  public void SetAutoOffsetReset()
  {
    //Arrange
    var autoOffsetReset = AutoOffsetReset.Latest;

    //Act
    ClassUnderTest.SetAutoOffsetReset(autoOffsetReset);

    //Assert
    ClassUnderTest.QueryStreamParameters.Get<AutoOffsetReset>(QueryStreamParameters.AutoOffsetResetPropertyName).Should().Be(autoOffsetReset);
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
