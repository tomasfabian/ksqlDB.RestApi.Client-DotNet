using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query
{
  public class KStreamSetDependenciesTests
  {
    [Test]
    public void QueryStreamParameters_CloneIsReturned()
    {
      //Arrange
      var queryStreamParameters = new QueryStreamParameters();
      queryStreamParameters.Set(QueryStreamParameters.AutoOffsetResetPropertyName, AutoOffsetReset.Latest);
      var kStreamSetDependencies = new KStreamSetDependencies(null!, null!, null!, queryStreamParameters);

      //Act
      var queryStreamParameters1 = kStreamSetDependencies.QueryStreamParameters;
      var queryStreamParameters2 = kStreamSetDependencies.QueryStreamParameters;

      //Assert
      queryStreamParameters1.Should().BeEquivalentTo(queryStreamParameters2);
    }

    [Test]
    public void QueryStreamParameters_CloneIsReturned_SqlIsChanged()
    {
      //Arrange
      var queryStreamParameters = new QueryStreamParameters();
      queryStreamParameters.Set(QueryStreamParameters.AutoOffsetResetPropertyName, AutoOffsetReset.Latest);
      var kStreamSetDependencies = new KStreamSetDependencies(null!, null!, null!, queryStreamParameters);

      //Act
      var queryStreamParameters1 = kStreamSetDependencies.QueryStreamParameters;
      queryStreamParameters1.Sql = "sql1";
      var queryStreamParameters2 = kStreamSetDependencies.QueryStreamParameters;
      queryStreamParameters1.Sql = "sql2";

      //Assert
      queryStreamParameters1.Should().NotBeEquivalentTo(queryStreamParameters2);
    }
  }
}
