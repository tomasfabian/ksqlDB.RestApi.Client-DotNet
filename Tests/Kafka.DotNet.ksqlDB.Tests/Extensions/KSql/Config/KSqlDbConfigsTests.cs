using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Config;
using NUnit.Framework;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Config
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
  }
}