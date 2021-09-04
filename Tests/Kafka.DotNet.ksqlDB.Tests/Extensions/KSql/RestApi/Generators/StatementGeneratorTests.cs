using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Generators;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Statements;
using NUnit.Framework;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Generators
{
  public class StatementGeneratorTests
  {    
    private static EntityCreationMetadata GetEntityCreationMetadata(string topicName)
    {
      EntityCreationMetadata metadata = new EntityCreationMetadata()
      {
        KeyFormat = SerializationFormats.Json,
        KafkaTopic = topicName,
        Partitions = 1,
        Replicas = 1,
        WindowType = WindowType.Tumbling,
        WindowSize = "10 SECONDS",
        TimestampFormat = "yyyy-MM-dd''T''HH:mm:ssX"
      };

      return metadata;
    }

    private string GetExpectedClauses(bool isTable)
    {
      var keyClause = isTable ? " PRIMARY" : string.Empty;

      return @$" MyMovies (
	Id INT{keyClause} KEY,
	Title VARCHAR,
	Release_Year INT,
	NumberOfDays ARRAY<INT>,
	Dictionary MAP<VARCHAR, INT>,
	Dictionary2 MAP<VARCHAR, INT>,
	Field DOUBLE
) WITH ( WINDOW_TYPE='Tumbling', WINDOW_SIZE='10 SECONDS', KAFKA_TOPIC='my_movie', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1', TIMESTAMP_FORMAT='yyyy-MM-dd''T''HH:mm:ssX' );";
    }

    [Test]
    public void CreateTable()
    {
      //Arrange
      var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

      //Act
      string statement = StatementGenerator.CreateTable<CreateEntityTests.MyMovie>(creationMetadata);

      //Assert
      statement.Should().Be($"CREATE TABLE{GetExpectedClauses(isTable: true)}");
    }

    [Test]
    public void CreateTable_IfNotExists()
    {
      //Arrange
      var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

      //Act
      string statement = StatementGenerator.CreateTable<CreateEntityTests.MyMovie>(creationMetadata, ifNotExists: true);

      //Assert
      statement.Should().Be($"CREATE TABLE IF NOT EXISTS{GetExpectedClauses(isTable: true)}");
    }

    [Test]
    public void CreateOrReplaceTable()
    {
      //Arrange
      var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

      //Act
      string statement = StatementGenerator.CreateOrReplaceTable<CreateEntityTests.MyMovie>(creationMetadata);

      //Assert
      statement.Should().Be($"CREATE OR REPLACE TABLE{GetExpectedClauses(isTable: true)}");
    }

    [Test]
    public void CreateStream()
    {
      //Arrange
      var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

      //Act
      string statement = StatementGenerator.CreateStream<CreateEntityTests.MyMovie>(creationMetadata);

      //Assert
      statement.Should().Be($"CREATE STREAM{GetExpectedClauses(isTable: false)}");
    }

    [Test]
    public void CreateStream_IfNotExists()
    {
      //Arrange
      var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

      //Act
      string statement = StatementGenerator.CreateStream<CreateEntityTests.MyMovie>(creationMetadata, ifNotExists: true);

      //Assert
      statement.Should().Be($"CREATE STREAM IF NOT EXISTS{GetExpectedClauses(isTable: false)}");
    }

    [Test]
    public void CreateOrReplaceStream()
    {
      //Arrange
      var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

      //Act
      string statement = StatementGenerator.CreateOrReplaceStream<CreateEntityTests.MyMovie>(creationMetadata);

      //Assert
      statement.Should().Be($"CREATE OR REPLACE STREAM{GetExpectedClauses(isTable: false)}");
    }
  }
}