using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Connectors;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements.Connectors;

public class ConnectorGeneratorTests : TestBase
{
  private IDictionary<string, string> CreateConfig()
  {
    return new Dictionary<string, string>()
    {
      { "key1", "value1"},
      { "key2", "value2"}
    };
  }

  readonly string connectorName = "myConnector";

  [Test]
  public void ToCreateSourceConnectorStatement()
  {
    //Arrange
    var connectorConfig = CreateConfig();

    //Act
    var statement = connectorConfig.ToCreateConnectorStatement(connectorName);

    //Assert
    statement.Should().Be(ExpectedStatement("CREATE SOURCE CONNECTOR"));
  }

  [Test]
  public void ToCreateSourceConnectorStatement_IfNotExists()
  {
    //Arrange
    var connectorConfig = CreateConfig();

    //Act
    var statement = connectorConfig.ToCreateConnectorStatement(connectorName, ifNotExists: true);

    //Assert
    statement.Should().Be(ExpectedStatement("CREATE SOURCE CONNECTOR IF NOT EXISTS"));
  }

  [Test]
  public void ToCreateSinkConnectorStatement()
  {
    //Arrange
    var connectorConfig = CreateConfig();

    //Act
    var statement = connectorConfig.ToCreateConnectorStatement(connectorName, ifNotExists: false, ConnectorType.Sink);
      
    //Assert
    statement.Should().Be(ExpectedStatement("CREATE SINK CONNECTOR"));
  }

  [Test]
  public void ToCreateSinkConnectorStatement_IfNotExists()
  {
    //Arrange
    var connectorConfig = CreateConfig();

    //Act
    var statement = connectorConfig.ToCreateConnectorStatement(connectorName, ifNotExists: true, ConnectorType.Sink);
      
    //Assert
    statement.Should().Be(ExpectedStatement("CREATE SINK CONNECTOR IF NOT EXISTS"));
  }

  private string ExpectedStatement(string create)
  {
    return @$"{create} `{connectorName}` WITH (
	'key1'= 'value1', 
	'key2'= 'value2'
);
";
  }
}
