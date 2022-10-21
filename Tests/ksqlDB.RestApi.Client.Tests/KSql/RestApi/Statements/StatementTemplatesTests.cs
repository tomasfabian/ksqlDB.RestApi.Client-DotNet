using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Statements;

public class StatementTemplatesTests
{
  [Test]
  public void ShowAllTopics()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowAllTopics;

    //Assert
    statement.Should().Be("SHOW ALL TOPICS;");
  }

  [Test]
  public void ShowAllTopicsExtended()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowAllTopicsExtended;

    //Assert
    statement.Should().Be("SHOW ALL TOPICS EXTENDED;");
  }

  [Test]
  public void ShowConnectors()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowConnectors;

    //Assert
    statement.Should().Be("SHOW CONNECTORS;");
  }

  [Test]
  public void ShowQueries()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowQueries;

    //Assert
    statement.Should().Be("SHOW QUERIES;");
  }

  [Test]
  public void ShowStreams()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowStreams;

    //Assert
    statement.Should().Be("SHOW STREAMS;");
  }

  [Test]
  public void ShowTables()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowTables;

    //Assert
    statement.Should().Be("SHOW TABLES;");
  }

  [Test]
  public void ShowTopics()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowTopics;

    //Assert
    statement.Should().Be("SHOW TOPICS;");
  }

  [Test]
  public void ShowTopicsExtended()
  {
    //Arrange

    //Act
    var statement = StatementTemplates.ShowTopicsExtended;

    //Assert
    statement.Should().Be("SHOW TOPICS EXTENDED;");
  }

  [Test]
  public void DropConnector()
  {
    //Arrange
    string connectorName = "CONNECTOR_NAME";

    //Act
    var statement = StatementTemplates.DropConnector(connectorName);

    //Assert
    statement.Should().Be($"DROP CONNECTOR {connectorName};");
  }

  [Test]
  public void DropStream()
  {
    //Arrange
    string streamName = "STREAM_NAME";

    //Act
    var statement = StatementTemplates.DropStream(streamName);

    //Assert
    statement.Should().Be($"DROP STREAM {streamName};");
  }

  [Test]
  public void DropStream_IfExists()
  {
    //Arrange
    string streamName = "STREAM_NAME";

    //Act
    var statement = StatementTemplates.DropStream(streamName, useIfExists: true, deleteTopic: false);

    //Assert
    statement.Should().Be($"DROP STREAM IF EXISTS {streamName};");
  }

  [Test]
  public void DropStream_DeleteTopic()
  {
    //Arrange
    string streamName = "STREAM_NAME";

    //Act
    var statement = StatementTemplates.DropStream(streamName, useIfExists: false, deleteTopic: true);

    //Assert
    statement.Should().Be($"DROP STREAM {streamName} DELETE TOPIC;");
  }

  [Test]
  public void DropStream_IfExistsAndDeleteTopic()
  {
    //Arrange
    string streamName = "STREAM_NAME";

    //Act
    var statement = StatementTemplates.DropStream(streamName, useIfExists: true, deleteTopic: true);

    //Assert
    statement.Should().Be($"DROP STREAM IF EXISTS {streamName} DELETE TOPIC;");
  }

  [Test]
  public void DropTable()
  {
    //Arrange
    string tableName = "TABLE_NAME";

    //Act
    var statement = StatementTemplates.DropTable(tableName);

    //Assert
    statement.Should().Be($"DROP TABLE {tableName};");
  }

  [Test]
  public void DropTable_IfExists()
  {
    //Arrange
    string tableName = "TABLE_NAME";

    //Act
    var statement = StatementTemplates.DropTable(tableName, useIfExists: true, deleteTopic: false);

    //Assert
    statement.Should().Be($"DROP TABLE IF EXISTS {tableName};");
  }

  [Test]
  public void DropTable_DeleteTopic()
  {
    //Arrange
    string tableName = "TABLE_NAME";

    //Act
    var statement = StatementTemplates.DropTable(tableName, useIfExists: false, deleteTopic: true);

    //Assert
    statement.Should().Be($"DROP TABLE {tableName} DELETE TOPIC;");
  }

  [Test]
  public void DropTable_IfExistsAndDeleteTopic()
  {
    //Arrange
    string tableName = "TABLE_NAME";

    //Act
    var statement = StatementTemplates.DropTable(tableName, useIfExists: true, deleteTopic: true);

    //Assert
    statement.Should().Be($"DROP TABLE IF EXISTS {tableName} DELETE TOPIC;");
  }

  [Test]
  public void TerminatePushQuery()
  {
    //Arrange
    string queryId = "QUERY_ID";

    //Act
    var statement = StatementTemplates.TerminatePersistentQuery(queryId);

    //Assert
    statement.Should().Be($"TERMINATE {queryId};");
  }

  [Test]
  public void PausePushQuery()
  {
    //Arrange
    string queryId = "QUERY_ID";

    //Act
    var statement = StatementTemplates.PausePersistentQuery(queryId);

    //Assert
    statement.Should().Be($"PAUSE {queryId};");
  }

  [Test]
  public void Explain()
  {
    //Arrange
    string sqlExpression = "SELECT * FROM My EMIT CHANGES;";

    //Act
    var statement = StatementTemplates.Explain(sqlExpression);

    //Assert
    statement.Should().Be($"EXPLAIN {sqlExpression}");
  }

  [Test]
  public void ExplainBy()
  {
    //Arrange
    string queryId = "QUERY_ID";

    //Act
    var statement = StatementTemplates.ExplainBy(queryId);

    //Assert
    statement.Should().Be($"EXPLAIN {queryId};");
  }

  [Test]
  public void DropType()
  {
    //Arrange
    string typeName = "MY_TYPE";

    //Act
    var statement = StatementTemplates.DropType(typeName);

    //Assert
    statement.Should().Be($"DROP TYPE {typeName};");
  }

  [Test]
  public void DropTypeIfExists()
  {
    //Arrange
    string typeName = "MY_TYPE";

    //Act
    var statement = StatementTemplates.DropTypeIfExists(typeName);

    //Assert
    statement.Should().Be($"DROP TYPE IF EXISTS {typeName};");
  }
}