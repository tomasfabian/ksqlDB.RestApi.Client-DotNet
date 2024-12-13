using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Generators;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Generators;

public class StatementGeneratorTests
{
  private readonly StatementGenerator statementGenerator = new();

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
	ReleaseYear INT,
	NumberOfDays ARRAY<INT>,
	Dictionary MAP<VARCHAR, INT>,
	Dictionary2 MAP<VARCHAR, INT>,
	Field DOUBLE
) WITH ( WINDOW_TYPE='Tumbling', WINDOW_SIZE='10 SECONDS', KAFKA_TOPIC='my_movie', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1', TIMESTAMP_FORMAT='yyyy-MM-dd''T''HH:mm:ssX' );".ReplaceLineEndings();
  }

  [Test]
  public void CreateTable()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

    //Act
    string statement = statementGenerator.CreateTable<CreateEntityTests.MyMovie>(creationMetadata);

    //Assert
    statement.Should().Be($"CREATE TABLE{GetExpectedClauses(isTable: true)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateTable_IfNotExists()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

    //Act
    string statement = statementGenerator.CreateTable<CreateEntityTests.MyMovie>(creationMetadata, ifNotExists: true);

    //Assert
    statement.Should().Be($"CREATE TABLE IF NOT EXISTS{GetExpectedClauses(isTable: true)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateOrReplaceTable()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

    //Act
    string statement = statementGenerator.CreateOrReplaceTable<CreateEntityTests.MyMovie>(creationMetadata);

    //Assert
    statement.Should().Be($"CREATE OR REPLACE TABLE{GetExpectedClauses(isTable: true)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateSourceTable()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");
    creationMetadata.IsReadOnly = true;

    //Act
    string statement = statementGenerator.CreateTable<CreateEntityTests.MyMovie>(creationMetadata);

    //Assert
    statement.Should().Be($"CREATE SOURCE TABLE{GetExpectedClauses(isTable: true)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateStream()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

    //Act
    string statement = statementGenerator.CreateStream<CreateEntityTests.MyMovie>(creationMetadata);

    //Assert
    statement.Should().Be($"CREATE STREAM{GetExpectedClauses(isTable: false)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateStream_IfNotExists()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

    //Act
    string statement = statementGenerator.CreateStream<CreateEntityTests.MyMovie>(creationMetadata, ifNotExists: true);

    //Assert
    statement.Should().Be($"CREATE STREAM IF NOT EXISTS{GetExpectedClauses(isTable: false)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateOrReplaceStream()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");

    //Act
    string statement = statementGenerator.CreateOrReplaceStream<CreateEntityTests.MyMovie>(creationMetadata);

    //Assert
    statement.Should().Be($"CREATE OR REPLACE STREAM{GetExpectedClauses(isTable: false)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateSourceStream()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: "my_movie");
    creationMetadata.IsReadOnly = true;

    //Act
    string statement = statementGenerator.CreateStream<CreateEntityTests.MyMovie>(creationMetadata);

    //Assert
    statement.Should().Be($"CREATE SOURCE STREAM{GetExpectedClauses(isTable: false)}".ReplaceLineEndings());
  }

  [Test]
  public void CreateOrReplaceTableWithEnumProperty()
  {
    //Arrange
    var creationMetadata = GetEntityCreationMetadata(topicName: nameof(Port).ToLower());

    //Act
    string statement = statementGenerator.CreateOrReplaceTable<Port>(creationMetadata);

    //Assert
    statement.Should().Contain($"{nameof(PortType)} VARCHAR");
  }
  private record KeyValuePair
  {
    public string Key { get; set; } = null!;
    public byte[] Value { get; set; } = null!;
  }

  private record Record
  {
    public KeyValuePair[] Headers { get; init; } = null!;
  }

  [Test]
  public void CreateTable_UseModelBuilder_WithFieldAsStruct()
  {
    //Arrange
    var modelBuilder = new ModelBuilder();
    modelBuilder.Entity<Record>()
      .Property(b => b.Headers)
      .AsStruct();

    var creationMetadata = new EntityCreationMetadata("my_topic", partitions: 3);

    //Act
    var statement = new StatementGenerator(modelBuilder).CreateTable<Record>(creationMetadata, ifNotExists: true);

    //Assert
    statement.Should().Be(@"CREATE TABLE IF NOT EXISTS Records (
	Headers ARRAY<STRUCT<Key VARCHAR, Value BYTES>>
) WITH ( KAFKA_TOPIC='my_topic', VALUE_FORMAT='Json', PARTITIONS='3' );".ReplaceLineEndings());
  }

  private record Child : Record
  {
    public First[] Firsts { get; init; } = null!;
  }

  private record First
  {
    public Second[] Seconds { get; init; } = null!;
  }

  private record Second
  {
    public string Test { get; init; } = null!;
  }

  [Test]
  public void CreateTable_UseModelBuilder_WithNestedArrayFieldAsStruct()
  {
    //Arrange
    var modelBuilder = new ModelBuilder();
    modelBuilder.Entity<Child>().Property(b => b.Headers).AsStruct();
    modelBuilder.Entity<Child>().Property(b => b.Firsts).AsStruct();
    modelBuilder.Entity<Child>().Property(b => b.Firsts.FirstOrDefault()!.Seconds).AsStruct();

    var creationMetadata = new EntityCreationMetadata("my_topic", partitions: 3);

    //Act
    var statement = new StatementGenerator(modelBuilder).CreateTable<Child>(
      creationMetadata,
      ifNotExists: true
    );

    //Assert
    statement
      .Should()
      .Be(
        @"CREATE TABLE IF NOT EXISTS Children (
	Firsts ARRAY<STRUCT<Seconds ARRAY<STRUCT<Test VARCHAR>>>>,
	Headers ARRAY<STRUCT<Key VARCHAR, Value BYTES>>
) WITH ( KAFKA_TOPIC='my_topic', VALUE_FORMAT='Json', PARTITIONS='3' );".ReplaceLineEndings()
      );
  }

  private class Child2
  {
    public First2[] Firsts { get; set; }
  }

  private class First2
  {
    public Second2[] Seconds { get; set; }
  }

  private class Second2
  {
    public Third[] Thirds { get; set; }
  }

  private class Third
  {
    public string Test1 { get; set; }
    public string Test2 { get; set; }
  }

  [Test]
  public void CreateTable_UseModelBuilder_WithNestedArrayFieldAsStruct_2()
  {
    //Arrange
    var modelBuilder = new ModelBuilder();
    modelBuilder.Entity<Child2>().Property(b => b.Firsts).AsStruct();
    modelBuilder.Entity<Child2>().Property(b => b.Firsts.FirstOrDefault()!.Seconds).AsStruct();
    modelBuilder.Entity<Child2>().Property(b => b.Firsts.FirstOrDefault()!.Seconds.FirstOrDefault()!.Thirds).AsStruct();

    var creationMetadata = new EntityCreationMetadata("my_topic", partitions: 3);

    //Act
    var statement = new StatementGenerator(modelBuilder).CreateTable<Child2>(
      creationMetadata,
      ifNotExists: true
    );

    //Assert
    statement
      .Should()
      .Be(
        @"CREATE TABLE IF NOT EXISTS Child2s (
	Firsts ARRAY<STRUCT<Seconds ARRAY<STRUCT<Thirds ARRAY<STRUCT<Test1 VARCHAR, Test2 VARCHAR>>>>>>
) WITH ( KAFKA_TOPIC='my_topic', VALUE_FORMAT='Json', PARTITIONS='3' );".ReplaceLineEndings()
      );
  }
}

internal class Port
{
  [Key]
  public int Id { get; set; }
  public PortType PortType { get; set; }
}

internal enum PortType
{
  Kafka = 0,
  Snowflake = 1,
}
