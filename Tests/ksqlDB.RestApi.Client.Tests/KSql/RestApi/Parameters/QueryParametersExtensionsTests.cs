using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parameters;

public class QueryParametersExtensionsTests
{
  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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
