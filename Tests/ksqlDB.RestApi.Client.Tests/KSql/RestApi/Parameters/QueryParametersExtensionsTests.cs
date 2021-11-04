using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Parameters
{
  [TestClass]
  public class QueryParametersExtensionsTests
  {
    [TestMethod]
    public void ToLogInfo_QueryStreamParameters_NoParameters()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters();

      //Act
      var result = queryParameters.ToLogInfo();

      //Assert
      result.Should().Be(@"Sql: 
Parameters:
");
    }

    [TestMethod]
    public void ToLogInfo_QueryStreamParameters()
    {
      //Arrange
      var queryParameters = new QueryStreamParameters
                            {
                              AutoOffsetReset = AutoOffsetReset.Earliest,
                              [KSqlDbConfigs.ProcessingGuarantee] = ProcessingGuarantee.AtLeastOnce.ToKSqlValue()
                            };

      //Act
      var result = queryParameters.ToLogInfo();

      //Assert
      result.Should().Be(@"Sql: 
Parameters:
auto.offset.reset = earliest
processing.guarantee = at_least_once
");
    }

    [TestMethod]
    public void ToLogInfo_QueryStreamParameters_Sql()
    {
      //Arrange
      string sql = "Select * From tweets Emit changes;";

      var queryParameters = new QueryStreamParameters
                            {
                              Sql = sql
                            };

      //Act
      var result = queryParameters.ToLogInfo();

      //Assert
      result.Should().Be($@"Sql: {sql}
Parameters:
");
    }

    [TestMethod]
    public void ToLogInfo_QueryParameters()
    {
      //Arrange
      var queryParameters = new QueryParameters
                            {
                              AutoOffsetReset = AutoOffsetReset.Earliest,
                              [KSqlDbConfigs.ProcessingGuarantee] = ProcessingGuarantee.AtLeastOnce.ToKSqlValue()
                            };

      //Act
      var result = queryParameters.ToLogInfo();

      //Assert
      result.Should().Be(@"Sql: 
Parameters:
ksql.streams.auto.offset.reset = earliest
processing.guarantee = at_least_once
");
    }
  }
}