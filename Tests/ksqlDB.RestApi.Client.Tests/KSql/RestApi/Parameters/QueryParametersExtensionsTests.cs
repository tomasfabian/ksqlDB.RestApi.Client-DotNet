using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parameters;

public class QueryParametersExtensionsTests
{
  [Test]
  public void FillFrom_DestinationParametersAreSetFromSource()
  {
    //Arrange
    var source = new QueryStreamParameters
    {
      Sql = "Select"
    };
    source.Set("key", "value");
    var destination = new QueryStreamParameters();

    //Act
    destination.FillFrom(source);

    //Assert
    destination.Sql.Should().BeEquivalentTo(source.Sql);
    destination.Properties.Count.Should().Be(source.Properties.Count);
  }

  [Test]
  public void FillPushQueryParametersFrom_DestinationParametersAreSetFromSource()
  {
    //Arrange
    var source = new QueryStreamParameters()
    {
      AutoOffsetReset = AutoOffsetReset.Latest
    };
    var destination = new QueryStreamParameters();

    //Act
    destination.FillPushQueryParametersFrom(source);

    //Assert
    destination.AutoOffsetReset.Should().Be(source.AutoOffsetReset);
  }

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
".ReplaceLineEndings());
  }

  [Test]
  public void ToLogInfo_QueryStreamParameters()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters
    {
      AutoOffsetReset = AutoOffsetReset.Earliest
    };
    queryParameters.Set(KSqlDbConfigs.ProcessingGuarantee, ProcessingGuarantee.AtLeastOnce);

    //Act
    var result = queryParameters.ToLogInfo();

    //Assert
    result.Should().Be(@"Sql:
Parameters:
auto.offset.reset = ""earliest""
processing.guarantee = ""at_least_once""
".ReplaceLineEndings());
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
".ReplaceLineEndings());
  }

  [Test]
  public void ToLogInfo_QueryParameters()
  {
    //Arrange
    var queryParameters = new QueryParameters
    {
      AutoOffsetReset = AutoOffsetReset.Earliest,
    };
    queryParameters.Set(KSqlDbConfigs.ProcessingGuarantee, ProcessingGuarantee.AtLeastOnce);

    //Act
    var result = queryParameters.ToLogInfo();

    //Assert
    result.Should().Be(@"Sql:
Parameters:
ksql.streams.auto.offset.reset = ""earliest""
processing.guarantee = ""at_least_once""
".ReplaceLineEndings());
  }
}
