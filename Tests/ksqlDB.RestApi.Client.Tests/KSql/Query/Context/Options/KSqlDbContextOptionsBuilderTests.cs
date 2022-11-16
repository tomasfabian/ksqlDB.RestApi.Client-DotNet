using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;
using static System.String;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Context.Options;

[TestClass]
public class KSqlDbContextOptionsBuilderTests : TestBase<KSqlDbContextOptionsBuilder>
{
  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = new KSqlDbContextOptionsBuilder();
  }

  [TestMethod]
  [ExpectedException(typeof(ArgumentNullException))]
  public void UseKSqlDb_NullUrl_ThrowsArgumentNullException()
  {
    //Arrange

    //Act
    var options = ClassUnderTest.UseKSqlDb(null).Options;

    //Assert
  }

  [TestMethod]
  [ExpectedException(typeof(ArgumentNullException))]
  public void UseKSqlDb_EmptyStringUrl_ThrowsArgumentNullException()
  {
    //Arrange

    //Act
    var options = ClassUnderTest.UseKSqlDb(Empty).Options;

    //Assert
  }

  [TestMethod]
  public void UseKSqlDb_OptionsContainsFilledUrl()
  {
    //Arrange

    //Act
    var options = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl).Options;

    //Assert
    options.Url.Should().BeEquivalentTo(TestParameters.KsqlDBUrl);
  }

  [TestMethod]
  public void SetBasicAuthCredentials()
  {
    //Arrange
    string userName = "fred";
    string password = "letmein";

    //Act
    var options = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl)
      .SetBasicAuthCredentials(userName, password).Options;

    //Assert
    options.UseBasicAuth.Should().BeTrue();
    options.BasicAuthUserName.Should().Be(userName);
    options.BasicAuthPassword.Should().Be(password);
  }

  [TestMethod]
  public void NotSetBasicAuthCredentials()
  {
    //Arrange

    //Act
    var options = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl).Options;

    //Assert
    options.UseBasicAuth.Should().BeFalse();
    options.BasicAuthUserName.Should().BeEmpty();
    options.BasicAuthPassword.Should().BeEmpty();
  }
    
  [TestMethod]
  public void SetProcessingGuarantee()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var options = setupParameters.SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce).Options;

    //Assert
    options.QueryParameters[KSqlDbConfigs.ProcessingGuarantee].Should().Be("at_least_once");
    options.QueryStreamParameters[KSqlDbConfigs.ProcessingGuarantee].Should().Be("at_least_once");
  }
    
  [TestMethod]
  public void SetProcessingGuarantee_ThenSetupQueryStream()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl)
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
    
  [TestMethod]
  public void SetAutoOffsetReset()
  {
    //Arrange
    var autoOffsetReset = AutoOffsetReset.Latest;

    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var options = setupParameters
      .SetAutoOffsetReset(autoOffsetReset).Options;

    //Assert
    string expectedValue = autoOffsetReset.ToString().ToLower();
    options.QueryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().Be(expectedValue);
    options.QueryStreamParameters[QueryStreamParameters.AutoOffsetResetPropertyName].Should().Be(expectedValue);
  }
    
  [TestMethod]
  public void SetJsonSerializerOptions_DefaultPropertyNameCaseInsensitiveIsTrue()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var options = setupParameters.Options;

    //Assert
    options.JsonSerializerOptions.PropertyNameCaseInsensitive.Should().BeTrue();
  }
    
  [TestMethod]
  public void SetJsonSerializerOptions()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var options = setupParameters
      .SetJsonSerializerOptions(c => c.PropertyNameCaseInsensitive = false).Options;

    //Assert
    options.JsonSerializerOptions.PropertyNameCaseInsensitive.Should().BeFalse();
  }

  #region QueryStream

  [TestMethod]
  public void SetupQueryStream_OptionsQueryStreamParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var options = setupParameters.SetupQueryStream(c =>
    {

    }).Options;

    //Assert
    options.QueryStreamParameters.Properties[QueryStreamParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo("earliest");
  }

  [TestMethod]
  public void SetupQueryStreamNotCalled_OptionsQueryStreamParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);
    string earliestAtoOffsetReset = AutoOffsetReset.Earliest.ToString().ToLower();

    //Act
    var options = setupParameters.Options;

    //Assert
    options.QueryStreamParameters.Properties[QueryStreamParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo(earliestAtoOffsetReset);
  }

  [TestMethod]
  public void SetupQueryStream_AmendOptionsQueryStreamParametersProperty_AutoOffsetResetWasChanged()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);
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

  [TestMethod]
  public void SetupQuery_OptionsQueryParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var options = setupParameters.SetupQuery(c =>
    {

    }).Options;

    //Assert
    options.QueryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo("earliest");
  }

  [TestMethod]
  public void SetupQueryNotCalled_OptionsQueryParameters_AutoOffsetResetIsSetToDefault()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);
    string earliestAtoOffsetReset = AutoOffsetReset.Earliest.ToString().ToLower();

    //Act
    var options = setupParameters.Options;

    //Assert
    options.QueryParameters.Properties[QueryParameters.AutoOffsetResetPropertyName].Should().BeEquivalentTo(earliestAtoOffsetReset);
  }

  [TestMethod]
  public void SetupQuery_AmendOptionsQueryParametersProperty_AutoOffsetResetWasChanged()
  {
    //Arrange
    var setupParameters = ClassUnderTest.UseKSqlDb(TestParameters.KsqlDBUrl);
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