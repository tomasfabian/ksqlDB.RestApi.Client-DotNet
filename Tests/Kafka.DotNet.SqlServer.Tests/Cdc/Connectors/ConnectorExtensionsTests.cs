using FluentAssertions;
using Kafka.DotNet.SqlServer.Cdc.Connectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.SqlServer.Tests.Cdc.Connectors
{
  [TestClass]
  public class ConnectorExtensionsTests : TestBase
  {
    private SqlServerConnectorMetadata CreateConnector()
    {
      string connectionString =
        "Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

      return new SqlServerConnectorMetadata(connectionString);
    }

    [TestMethod]
    public void ToStatement()
    {
      //Arrange
      var connector = CreateConnector();
      string connectorName = "myConnector";

      //Act
      var statement = connector.ToStatement(connectorName);

      //Assert
      statement.Should().Be(@$"CREATE SOURCE CONNECTOR {connectorName} WITH (
	'connector.class'= 'io.debezium.connector.sqlserver.SqlServerConnector', 
	'database.port'= '1433', 
	'database.hostname'= '127.0.0.1', 
	'database.user'= 'SA', 
	'database.password'= '<YourNewStrong@Passw0rd>', 
	'database.dbname'= 'Sensors'
);
");
    }
  }
}