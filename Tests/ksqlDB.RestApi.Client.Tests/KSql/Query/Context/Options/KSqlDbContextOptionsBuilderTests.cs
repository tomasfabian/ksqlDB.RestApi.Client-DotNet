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
  public void UseKSqlDb_EmptyStringUrl_ThrowsArgumentNullException()
  {
    //Arrange

    //Assert
    Assert.ThrowsException<ArgumentNullException>(() =>
    {
      //Act
      _ = ClassUnderTest.UseKSqlDb(Empty).Options;
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
    options.QueryStreamParameters.Get<ProcessingGuarantee>(KSqlDbConfigs.ProcessingGuarantee).Should().Be(ProcessingGuarantee.AtLeastOnce);
  }

  [Test]
  public void SetProcessingGuarantee_ThenSetupPushQuery()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl)
      .SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce);

    //Act
    var options = setupParameters
      .SetupPushQuery(_ =>
      {
      }).Options;

    //Assert
    options.QueryStreamParameters.Get<ProcessingGuarantee>(KSqlDbConfigs.ProcessingGuarantee).Should().Be(ProcessingGuarantee.AtLeastOnce);
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
    options.QueryStreamParameters.Get<AutoOffsetReset>(QueryStreamParameters.AutoOffsetResetPropertyName).Should().Be(autoOffsetReset);
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

  #region SetupPushQuery

  [Test]
  public void SetupPushQuery_OptionsQueryStreamParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.SetupPushQuery(_ =>
    {
    }).Options;

    //Assert
    options.QueryStreamParameters.Get<AutoOffsetReset>(QueryStreamParameters.AutoOffsetResetPropertyName).Should().Be(AutoOffsetReset.Earliest);
  }

  [Test]
  public void SetupPushQueryNotCalled_OptionsQueryStreamParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.Options;

    //Assert
    options.QueryStreamParameters.Get<AutoOffsetReset>(QueryStreamParameters.AutoOffsetResetPropertyName).Should().Be(AutoOffsetReset.Earliest);
  }

  [Test]
  public void SetupPushQuery_AmendOptionsQueryStreamParametersProperty_AutoOffsetResetWasChanged()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);
    string latestAtoOffsetReset = AutoOffsetReset.Latest.ToString().ToLower();

    //Act
    var options = setupParameters.SetupPushQuery(c =>
    {
      c.Set(QueryStreamParameters.AutoOffsetResetPropertyName, AutoOffsetReset.Latest);
    }).Options;

    //Assert
    options.QueryStreamParameters.Get<AutoOffsetReset>(QueryStreamParameters.AutoOffsetResetPropertyName)
      .ToString()
      .ToLower()
      .Should().Be(latestAtoOffsetReset);
  }

  [Test]
  public void SetEndpointType_DefaultQueryStreamWasSet()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.Options;

    //Assert
    options.EndpointType.Should().Be(EndpointType.QueryStream);
  }

  public static EndpointType[] SetEndpointTypeTestCases() => Enum.GetValues<EndpointType>();

  [TestCaseSource(nameof(SetEndpointTypeTestCases))]
  public void SetEndpointType_QueryWasSet(EndpointType endpointType)
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl)
      .SetEndpointType(endpointType);

    //Act
    var options = setupParameters.Options;

    //Assert
    options.EndpointType.Should().Be(endpointType);
  }

  #endregion

  #region SetupPullQuery

  [Test]
  public void SetupPullQuery_PropertyWasSet()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = setupParameters.SetupPullQuery(opt =>
    {
      opt.Set(KSqlDbConfigs.KsqlQueryPullTableScanEnabled, true);
    }).Options;

    //Assert
    options.PullQueryParameters.Get<bool>(KSqlDbConfigs.KsqlQueryPullTableScanEnabled).Should().Be(true);
  }
  #endregion
}
