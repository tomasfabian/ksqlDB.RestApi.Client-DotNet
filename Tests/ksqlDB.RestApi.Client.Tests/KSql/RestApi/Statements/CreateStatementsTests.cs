using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Statements;

public class CreateStatementsTests
{
  [Test]
  public void GenerateWithClause_KafkaTopic()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      KafkaTopic = "tweetsByTitle"
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( KAFKA_TOPIC='{metadata.KafkaTopic}' )");
  }

  [Test]
  public void GenerateWithClause_KeyFormat()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      KeyFormat = SerializationFormats.Json,
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( KEY_FORMAT='{metadata.KeyFormat}' )");
  }

  [Test]
  public void GenerateWithClause_ValueFormat()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      ValueFormat = SerializationFormats.Json,
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( VALUE_FORMAT='{metadata.ValueFormat}' )");
  }

  [Test]
  public void GenerateWithClause_Partitions()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      Partitions = 3
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( PARTITIONS='{metadata.Partitions}' )");
  }

  [Test]
  public void GenerateWithClause_Replicas()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      Replicas = 3
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( REPLICAS='{metadata.Replicas}' )");
  }

  [Test]
  public void GenerateWithClause_ValueDelimiter()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      ValueDelimiter = "SPACE"
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( VALUE_DELIMITER='{metadata.ValueDelimiter}' )");
  }

  [Test]
  public void GenerateWithClause_Timestamp()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      Timestamp = "t2"
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( TIMESTAMP='{metadata.Timestamp}' )");
  }

  [Test]
  public void GenerateWithClause_TimestampFormat()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      TimestampFormat = "yyyy-MM-dd''T''HH:mm:ssX"
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( TIMESTAMP_FORMAT='{metadata.TimestampFormat}' )");
  }

  [Test]
  public void GenerateWithClause_WrapSingleValue()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      WrapSingleValue = true
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( WRAP_SINGLE_VALUE='{metadata.WrapSingleValue}' )");
  }

  [Test]
  public void GenerateWithClause_WindowType()
  {
    //Arrange
    var metadata = new EntityCreationMetadata
    {
      WindowType = WindowType.Hopping
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( WINDOW_TYPE='{metadata.WindowType}', VALUE_FORMAT='Json' )");
  }

  [Test]
  public void GenerateWithClause_WindowSize()
  {
    //Arrange
    var metadata = new EntityCreationMetadata
    {
      WindowSize = "10 MINUTES"
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( WINDOW_SIZE='{metadata.WindowSize}', VALUE_FORMAT='Json' )");
  }

  [Test]
  public void GenerateWithClause_MultipleValues_AreSeparated()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      TimestampFormat = "yyyy-MM-dd''T''HH:mm:ssX",
      WrapSingleValue = true
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( TIMESTAMP_FORMAT='{metadata.TimestampFormat}', WRAP_SINGLE_VALUE='{metadata.WrapSingleValue}' )");
  }

  [Test]
  public void GenerateWithClause_KeySchemaId()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      KeySchemaId = 1
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( KEY_SCHEMA_ID={metadata.KeySchemaId} )");
  }

  [Test]
  public void GenerateWithClause_ValueSchemaId()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      ValueSchemaId = 2
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( VALUE_SCHEMA_ID={metadata.ValueSchemaId} )");
  }

  [Test]
  public void GenerateWithClause_KeySchemaFullName()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      KeySchemaFullName = "ProductKey"
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( KEY_SCHEMA_FULL_NAME={metadata.KeySchemaFullName} )");
  }

  [Test]
  public void GenerateWithClause_ValueSchemaFullName()
  {
    //Arrange
    var metadata = new CreationMetadata
    {
      ValueSchemaFullName = "ProductInfo"
    };

    //Act
    var withClause = CreateStatements.GenerateWithClause(metadata);

    //Assert
    withClause.Should().BeEquivalentTo(@$" WITH ( VALUE_SCHEMA_FULL_NAME={metadata.ValueSchemaFullName} )");
  }
}
