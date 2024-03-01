using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using UnitTests;
using static System.String;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Context.Options;

public class KSqlDbContextOptionsBuilderTests : TestBase<KSqlDbContextOptionsBuilder>
{
  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = new KSqlDbContextOptionsBuilder();
  }

  [Test]
  public void UseKSqlDb_NullUrl_ThrowsArgumentNullException()
  {
    //Arrange

    //Assert
    Assert.ThrowsException<ArgumentNullException>(() =>
    {
      //Act
      var options = ClassUnderTest.UseKSqlDb(null).Options;
    });
  }

  [Test]
  public void UseKSqlDb_EmptyStringUrl_ThrowsArgumentNullException()
  {
    //Arrange

    //Assert
    Assert.ThrowsException<ArgumentNullException>(() =>
    {
      //Act
      var options = ClassUnderTest.UseKSqlDb(Empty).Options;
    });
  }

  [Test]
  public void UseKSqlDb_OptionsContainsFilledUrl()
  {
    //Arrange

    //Act
    var options = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl).Options;

    //Assert
    options.Url.Should().BeEquivalentTo(TestParameters.KsqlDbUrl);
  }

  [Test]
  public void SetBasicAuthCredentials()
  {
    //Arrange
    string userName = "fred";
    string password = "letmein";

    //Act
    var options = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl)
      .SetBasicAuthCredentials(userName, password).Options;

    //Assert
    options.UseBasicAuth.Should().BeTrue();
    options.BasicAuthUserName.Should().Be(userName);
    options.BasicAuthPassword.Should().Be(password);
  }

  [Test]
  public void NotSetBasicAuthCredentials()
  {
    //Arrange

    //Act
    var options = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl).Options;

    //Assert
    options.UseBasicAuth.Should().BeFalse();
    options.BasicAuthUserName.Should().BeEmpty();
    options.BasicAuthPassword.Should().BeEmpty();
  }

  [Test]
  public void SetProcessingGuarantee()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce).Options;

    //Assert
    options.QueryParameters[KSqlDbConfigs.ProcessingGuarantee].Should().Be("at_least_once");
    options.QueryStreamParameters[KSqlDbConfigs.ProcessingGuarantee].Should().Be("at_least_once");
  }

  [Test]
  public void SetProcessingGuarantee_ThenSetupQueryStream()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl)
      .SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce);

    //Act
    var options = setupParameters
      .SetupQueryStream(options =>
      {
      }).Options;

    //Assert
    options.QueryParameters[KSqlDbConfigs.ProcessingGuarantee].Should().Be("at_least_once");
    options.QueryStreamParameters[KSqlDbConfigs.ProcessingGuarantee].Should().Be("at_least_once");
  }

  [Test]
  public void SetAutoOffsetReset()
  {
    //Arrange
    var autoOffsetReset = AutoOffsetReset.Latest;

    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters
      .SetAutoOffsetReset(autoOffsetReset).Options;

    //Assert
    string expectedValue = autoOffsetReset.ToString().ToLower();
    options.QueryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().Be(expectedValue);
    options.QueryStreamParameters[QueryStreamParameters.AutoOffsetResetPropertyName].Should().Be(expectedValue);
  }

  [Test]
  public void SetJsonSerializerOptions_DefaultPropertyNameCaseInsensitiveIsTrue()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.Options;

    //Assert
    options.JsonSerializerOptions.PropertyNameCaseInsensitive.Should().BeTrue();
  }

  [Test]
  public void SetJsonSerializerOptions()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters
      .SetJsonSerializerOptions(c => c.PropertyNameCaseInsensitive = false).Options;

    //Assert
    options.JsonSerializerOptions.PropertyNameCaseInsensitive.Should().BeFalse();
  }

  public static IdentifierEscaping[] SetIdentifierEscapingTestCases() => Enum.GetValues<IdentifierEscaping>();

  [TestCaseSource(nameof(SetIdentifierEscapingTestCases))]
  public void SetIdentifierEscaping(IdentifierEscaping escaping)
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    setupParameters.SetIdentifierEscaping(escaping);

    //Assert
    ((KSqlDbContextOptionsBuilder)setupParameters).InternalOptions.IdentifierEscaping.Should().Be(escaping);
  }

  #region QueryStream

  [Test]
  public void SetupQueryStream_OptionsQueryStreamParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.SetupQueryStream(c =>
    {

    }).Options;

    //Assert
    options.QueryStreamParameters.Properties[QueryStreamParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo("earliest");
  }

  [Test]
  public void SetupQueryStreamNotCalled_OptionsQueryStreamParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);
    string earliestAtoOffsetReset = AutoOffsetReset.Earliest.ToString().ToLower();

    //Act
    var options = setupParameters.Options;

    //Assert
    options.QueryStreamParameters.Properties[QueryStreamParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo(earliestAtoOffsetReset);
  }

  [Test]
  public void SetupQueryStream_AmendOptionsQueryStreamParametersProperty_AutoOffsetResetWasChanged()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);
    string latestAtoOffsetReset = AutoOffsetReset.Latest.ToString().ToLower();

    //Act
    var options = setupParameters.SetupQueryStream(c =>
    {
      c.Properties[QueryStreamParameters.AutoOffsetResetPropertyName] = latestAtoOffsetReset;
    }).Options;

    //Assert
    options.QueryStreamParameters.Properties[QueryStreamParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo(latestAtoOffsetReset);
  }

  #endregion

  #region Query

  [Test]
  public void SetupQuery_OptionsQueryParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.SetupQuery(c =>
    {

    }).Options;

    //Assert
    options.QueryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo("earliest");
  }

  [Test]
  public void SetupQueryNotCalled_OptionsQueryParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);
    string earliestAtoOffsetReset = AutoOffsetReset.Earliest.ToString().ToLower();

    //Act
    var options = setupParameters.Options;

    //Assert
    options.QueryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo(earliestAtoOffsetReset);
  }

  [Test]
  public void SetupQuery_AmendOptionsQueryParametersProperty_AutoOffsetResetWasChanged()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);
    string latestAtoOffsetReset = AutoOffsetReset.Latest.ToString().ToLower();

    //Act
    var options = setupParameters.SetupQuery(c =>
    {
      c.Properties[QueryParameters.AutoOffsetResetPropertyName] = latestAtoOffsetReset;
    }).Options;

    //Assert
    options.QueryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo(latestAtoOffsetReset);
  }

  #endregion
}
