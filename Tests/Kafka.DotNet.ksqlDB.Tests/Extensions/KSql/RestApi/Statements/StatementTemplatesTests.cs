using System;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using NUnit.Framework;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Statements
{
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
    public void TerminatePushQuery()
    {
      //Arrange
      string queryId = "QUERY_ID";

      //Act
      var statement = StatementTemplates.TerminatePushQuery(queryId);

      //Assert
      statement.Should().Be($"TERMINATE {queryId};");
    }
  }
}