using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Config;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.Config
{
  public class KSqlDbConfigsTests
  {
    [Test]
    public void KsqlQueryPullTableScanEnabled()
    {
      //Arrange

      //Act
      var config = KSqlDbConfigs.KsqlQueryPullTableScanEnabled;

      //Assert
      config.Should().Be("ksql.query.pull.table.scan.enabled");
    }

    [Test]
    public void ProcessingGuarantee()
    {
      //Arrange

      //Act
      var config = KSqlDbConfigs.ProcessingGuarantee;

      //Assert
      config.Should().Be("processing.guarantee");
    }
  }
}