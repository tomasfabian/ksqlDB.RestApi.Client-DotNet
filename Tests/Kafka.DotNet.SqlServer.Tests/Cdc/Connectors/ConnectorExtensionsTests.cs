using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Connector.Cdc.Connectors;
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
    
    string connectorName = "myConnector";

    [TestMethod]
    public void ToCreateSourceConnectorStatement()
    {
      //Arrange
      var connector = CreateConnector();

      //Act
      var statement = connector.ToCreateConnectorStatement(connectorName);

      //Assert
      statement.Should().Be(ExpectedStatement("CREATE SOURCE CONNECTOR"));
    }

    [TestMethod]
    public void ToCreateSourceConnectorStatement_IfNotExists()
    {
      //Arrange
      var connector = CreateConnector();

      //Act
      var statement = connector.ToCreateConnectorStatement(connectorName, ifNotExists: true);

      //Assert
      statement.Should().Be(ExpectedStatement("CREATE SOURCE CONNECTOR IF NOT EXISTS"));
    }

    [TestMethod]
    public void ToCreateSinkConnectorStatement()
    {
      //Arrange
      var connector = CreateConnector();
      connector.ConnectorType = ConnectorType.Sink;

      //Act
      var statement = connector.ToCreateConnectorStatement(connectorName);
      
      //Assert
      statement.Should().Be(ExpectedStatement("CREATE SINK CONNECTOR"));
    }

    private string ExpectedStatement(string create)
    {
      return @$"{create} {connectorName} WITH (
	'connector.class'= 'io.debezium.connector.sqlserver.SqlServerConnector', 
	'database.port'= '1433', 
	'database.hostname'= '127.0.0.1', 
	'database.user'= 'SA', 
	'database.password'= '<YourNewStrong@Passw0rd>', 
	'database.dbname'= 'Sensors'
);
";
    }
  }
}