using System.Text.Json.Serialization;
using FluentAssertions;
using Joker.Extensions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using NUnit.Framework;
using Pluralize.NET;
using static ksqlDB.RestApi.Client.KSql.RestApi.Enums.IdentifierEscaping;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements;

public class CreateEntityTests
{
  private EntityCreationMetadata creationMetadata = null!;
  private readonly ModelBuilder modelBuilder = new();

  [SetUp]
  public void Init()
  {
    creationMetadata = new EntityCreationMetadata()
    {
      KafkaTopic = nameof(MyMovie),
      Partitions = 1,
      Replicas = 1
    };
  }

  private static readonly Pluralizer EnglishPluralizationService = new();

  private static string CreateExpectedStatement(string creationClause, bool hasPrimaryKey, string? entityName = null, IdentifierEscaping escaping = Never)
  {
    string key = hasPrimaryKey ? "PRIMARY KEY" : "KEY";

    if (entityName.IsNullOrEmpty())
      entityName = EnglishPluralizationService.Pluralize(nameof(MyMovie));

    return escaping switch
    {
      Never => @$"{creationClause} {entityName} (
	Id INT {key},
	Title VARCHAR,
	Release_Year INT,
	NumberOfDays ARRAY<INT>,
	Dictionary MAP<VARCHAR, INT>,
	Dictionary2 MAP<VARCHAR, INT>,
	Field DOUBLE
) WITH ( KAFKA_TOPIC='{nameof(MyMovie)}', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings(),
      Keywords => @$"{creationClause} {entityName} (
	Id INT {key},
	Title VARCHAR,
	Release_Year INT,
	NumberOfDays ARRAY<INT>,
	Dictionary MAP<VARCHAR, INT>,
	Dictionary2 MAP<VARCHAR, INT>,
	Field DOUBLE
) WITH ( KAFKA_TOPIC='{nameof(MyMovie)}', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings(),
      Always => @$"{creationClause} `{entityName}` (
	`Id` INT {key},
	`Title` VARCHAR,
	`Release_Year` INT,
	`NumberOfDays` ARRAY<INT>,
	`Dictionary` MAP<VARCHAR, INT>,
	`Dictionary2` MAP<VARCHAR, INT>,
	`Field` DOUBLE
) WITH ( KAFKA_TOPIC='{nameof(MyMovie)}', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings(),
      _ => throw new ArgumentOutOfRangeException(nameof(escaping), escaping, "Non-exhaustive match")
    };
  }

  internal static IEnumerable<(IdentifierEscaping, string)> PrintCreateStreamTestCases()
  {
    yield return (Never, CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, escaping: Never));
    yield return (Keywords, CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, escaping: Keywords));
    yield return (Always, CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, escaping: Always));
  }

  [TestCaseSource(nameof(PrintCreateStreamTestCases))]
  public void Print_CreateStream((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };
    creationMetadata.IdentifierEscaping = escaping;
    //Act
    var statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  private class Transaction
  {
    [Decimal(3,2)]
    public decimal Amount { get; set; }
  }

  internal static IEnumerable<(IdentifierEscaping, string)> DecimalWithPrecisionTestCases()
  {
    yield return (Never, $@"CREATE STREAM Transactions (
{"\t"}Amount DECIMAL(3,2)
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings());
    yield return (Keywords, $@"CREATE STREAM Transactions (
{"\t"}Amount DECIMAL(3,2)
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings());
    yield return (Always, $@"CREATE STREAM `Transactions` (
{"\t"}`Amount` DECIMAL(3,2)
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(DecimalWithPrecisionTestCases))]
  public void DecimalWithPrecision((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };
    creationMetadata.IdentifierEscaping = escaping;

    //Act
    var statement = new CreateEntity(modelBuilder).Print<Transaction>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  internal static IEnumerable<(IdentifierEscaping, string)> PrintCreatStreamOverrideEntityNameTestCases()
  {
    yield return (Never,
      CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false,
        entityName: EnglishPluralizationService.Pluralize("TestName")));
    yield return (Keywords,
      CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false,
        entityName: EnglishPluralizationService.Pluralize("TestName"), Keywords));
    yield return (Always,
      CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false,
        entityName: EnglishPluralizationService.Pluralize("TestName"), Always));
  }

  [TestCaseSource(nameof(PrintCreatStreamOverrideEntityNameTestCases))]
  public void Print_CreateStream_OverrideEntityName((IdentifierEscaping escaping, string expected)  testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    creationMetadata.EntityName = "TestName";
    creationMetadata.IdentifierEscaping = escaping;

    //Act
    var statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  internal static IEnumerable<(IdentifierEscaping, string)> PrintCreateStreamOverrideEntityNameDoNotPluralizeTestCases()
  {
    yield return (Never, CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, entityName: "TestName"));
    yield return (Keywords,
      CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, entityName: "TestName", Keywords));
    yield return (Always,
      CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, entityName: "TestName", Always));
  }

  [TestCaseSource(nameof(PrintCreateStreamOverrideEntityNameDoNotPluralizeTestCases))]
  public void Print_CreateStream_OverrideEntityName_DonNotPluralize((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    creationMetadata.ShouldPluralizeEntityName = false;
    creationMetadata.EntityName = "TestName";
    creationMetadata.IdentifierEscaping = escaping;

    //Act
    var statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  [Test]
  public void Print_CreateStream_DoNotPluralize()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false));
  }

  [Test]
  public void Print_CreateStream_WithIfNotExists()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, ifNotExists: true);

    //Assert
    statement.Should().Be(CreateExpectedStatement("CREATE STREAM IF NOT EXISTS", hasPrimaryKey: false));
  }

  [Test]
  public void Print_CreateOrReplaceStream()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.CreateOrReplace,
      KSqlEntityType = KSqlEntityType.Stream
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(CreateExpectedStatement("CREATE OR REPLACE STREAM", hasPrimaryKey: false));
  }

  [Test]
  public void Print_CreateTable()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Table
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(CreateExpectedStatement("CREATE TABLE", hasPrimaryKey: true));
  }

  [Test]
  public void Print_CreateTable_WithIfNotExists()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Table
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, ifNotExists: true);

    //Assert
    statement.Should().Be(CreateExpectedStatement("CREATE TABLE IF NOT EXISTS", hasPrimaryKey: true));
  }

  [Test]
  public void Print_CreateOrReplaceTable()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.CreateOrReplace,
      KSqlEntityType = KSqlEntityType.Table
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(CreateExpectedStatement("CREATE OR REPLACE TABLE", hasPrimaryKey: true));
  }

  internal static IEnumerable<(IdentifierEscaping, string)> PrintCreateOrReplaceTableIncludeReadOnlyPropertiesTestCases()
  {
    yield return (Never, $@"CREATE OR REPLACE TABLE MyItems (
{"\t"}Id INT PRIMARY KEY,
{"\t"}Items ARRAY<INT>
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings());
    yield return (Keywords, $@"CREATE OR REPLACE TABLE MyItems (
{"\t"}Id INT PRIMARY KEY,
{"\t"}Items ARRAY<INT>
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings());
    yield return (Always, $@"CREATE OR REPLACE TABLE `MyItems` (
{"\t"}`Id` INT PRIMARY KEY,
{"\t"}`Items` ARRAY<INT>
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings());
  }

  [TestCaseSource(nameof(PrintCreateOrReplaceTableIncludeReadOnlyPropertiesTestCases))]
  public void Print_CreateOrReplaceTable_IncludeReadOnlyProperties((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var statementContext = new StatementContext
    {
      CreationType = CreationType.CreateOrReplace,
      KSqlEntityType = KSqlEntityType.Table
    };

    creationMetadata.IncludeReadOnlyProperties = true;
    creationMetadata.IdentifierEscaping = escaping;

    //Act
    var statement = new CreateEntity(modelBuilder).Print<MyItems>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  public abstract record AbstractProducerClass
  {
    [Key]
    public string Key { get; set; } = null!;
  }

  record Enrichedevent1 : AbstractProducerClass
  {
    public EventCategory[] EventCategories { get; set; } = null!;
  }

  [TestCase(Never)]
  [TestCase(Keywords)]
  [TestCase(Always)]
  public void Print_NestedArrayType_CreateTableIfNotExists(IdentifierEscaping escaping)
  {
    TestCreateEntityWithEnumerable<Enrichedevent1>(escaping: escaping);
  }

  record Enrichedevent2 : AbstractProducerClass
  {
    public List<EventCategory> EventCategories { get; set; } = null!;
  }

  [TestCase(Never)]
  [TestCase(Keywords)]
  [TestCase(Always)]
  public void Print_NestedListType_CreateTableIfNotExists(IdentifierEscaping escaping)
  {
    TestCreateEntityWithEnumerable<Enrichedevent2>(escaping: escaping);
  }

  record Enrichedevent3 : AbstractProducerClass
  {
    public IEnumerable<EventCategory> EventCategories { get; set; } = null!;
  }

  [TestCase(Never)]
  [TestCase(Keywords)]
  [TestCase(Always)]
  public void Print_NestedGenericEnumerableType_CreateTableIfNotExists(IdentifierEscaping escaping)
  {
    TestCreateEntityWithEnumerable<Enrichedevent3>(escaping: escaping);
  }

  record Enrichedevent4 : AbstractProducerClass
  {
    public int[] EventCategories { get; set; } = null!;
  }

  [TestCase(Never)]
  [TestCase(Keywords)]
  [TestCase(Always)]
  public void Print_NestedPrimitiveArrayType_CreateTableIfNotExists(IdentifierEscaping escaping)
  {
    TestCreateEntityWithEnumerable<Enrichedevent4>(arrayElementType: "INT", escaping);
  }

  //CREATE TYPE EventCategories AS STRUCT<id INTEGER, name VARCHAR, description VARCHAR>;
  record EventCategory
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
  }

  private void TestCreateEntityWithEnumerable<TEntity>(string arrayElementType = "EVENTCATEGORY", IdentifierEscaping escaping = Never)
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Table
    };

    var creationMetadata = new EntityCreationMetadata()
    {
      EntityName = "Enrichedevents",
      KafkaTopic = "enrichedevents",
      Partitions = 1,
      IdentifierEscaping = escaping
    };

    //Act
    var statement = new CreateEntity(modelBuilder).Print<TEntity>(statementContext, creationMetadata, true);

    //Assert
    switch (escaping)
    {
      case Never:
      case Keywords:
        statement.Should().Be(@$"CREATE TABLE IF NOT EXISTS Enrichedevents (
	EventCategories ARRAY<{arrayElementType}>,
	Key VARCHAR PRIMARY KEY
) WITH ( KAFKA_TOPIC='enrichedevents', VALUE_FORMAT='Json', PARTITIONS='1' );".ReplaceLineEndings());
        break;
      case Always:
        statement.Should().Be(@$"CREATE TABLE IF NOT EXISTS `Enrichedevents` (
	`EventCategories` ARRAY<{arrayElementType}>,
	`Key` VARCHAR PRIMARY KEY
) WITH ( KAFKA_TOPIC='enrichedevents', VALUE_FORMAT='Json', PARTITIONS='1' );".ReplaceLineEndings());
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(escaping), escaping, "Non-exhaustive match");
    }
  }

  internal class MyMovie
  {
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int Release_Year { get; set; }

    public int[] NumberOfDays { get; init; } = null!;

    public IDictionary<string, int> Dictionary { get; set; } = null!;
    public Dictionary<string, int> Dictionary2 { get; set; } = null!;
//#pragma warning disable CS0649
    public double Field;
//#pragma warning restore CS0649
    public int DontFindMe { get; }
  }

  internal class MyItems
  {
    [Key]
    public int Id { get; set; }

    public IEnumerable<int> Items { get; } = [];
  }

  internal record TimeTypes
  {
    public DateTime Dt { get; set; }
    public TimeSpan Ts { get; set; }
    public DateTimeOffset DtOffset { get; set; }
  }

  [Test]
  public void TestCreateEntityWithEnumerable()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    var streamCreationMetadata = new EntityCreationMetadata()
    {
      EntityName = nameof(TimeTypes),
      KafkaTopic = "enrichedevents",
      Partitions = 1
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<TimeTypes>(statementContext, streamCreationMetadata, false);

    //Assert
    statement.Should().Be(@$"CREATE STREAM {nameof(TimeTypes)} (
	Dt DATE,
	Ts TIME,
	DtOffset TIMESTAMP
) WITH ( KAFKA_TOPIC='enrichedevents', VALUE_FORMAT='Json', PARTITIONS='1' );".ReplaceLineEndings());
  }

  internal record Renamed
  {
    [JsonPropertyName("data_id")]
    public string DataId { get; set; } = null!;
  }

  [Test]
  public void JsonPropertyName()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    var streamCreationMetadata = new EntityCreationMetadata()
    {
      EntityName = nameof(Renamed),
      KafkaTopic = "Renamed_values",
      Partitions = 1,
      ShouldPluralizeEntityName = false
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<Renamed>(statementContext, streamCreationMetadata, false);

    //Assert
    statement.Should().Be(@$"CREATE STREAM {nameof(Renamed)} (
	data_id VARCHAR
) WITH ( KAFKA_TOPIC='{streamCreationMetadata.KafkaTopic}', VALUE_FORMAT='Json', PARTITIONS='1' );".ReplaceLineEndings());
  }

  internal record GuidKey
  {
    public Guid DataId { get; set; }
  }

  [Test]
  public void GuidToVarcharProperty()
  {
    //Arrange
    var statementContext = new StatementContext
    {
      CreationType = CreationType.Create,
      KSqlEntityType = KSqlEntityType.Stream
    };

    var streamCreationMetadata = new EntityCreationMetadata()
    {
      EntityName = nameof(GuidKey),
      KafkaTopic = "guid_key",
      Partitions = 1,
      ShouldPluralizeEntityName = false
    };

    //Act
    string statement = new CreateEntity(modelBuilder).Print<GuidKey>(statementContext, streamCreationMetadata, false);

    //Assert
    statement.Should().Be(@$"CREATE STREAM {nameof(GuidKey)} (
	{nameof(GuidKey.DataId)} VARCHAR
) WITH ( KAFKA_TOPIC='{streamCreationMetadata.KafkaTopic}', VALUE_FORMAT='Json', PARTITIONS='1' );".ReplaceLineEndings());
  }

  internal class Poco
  {
    public int Id { get; set; }
    public string Description { get; set; } = null!;
  }

  [Test]
  public void ModelBuilder_EntityHasKey()
  {
    //Arrange
    modelBuilder.Entity<Poco>()
      .HasKey(x => x.Id);

    var statementContext = new StatementContext
    {
      CreationType = CreationType.CreateOrReplace,
      KSqlEntityType = KSqlEntityType.Table,
    };

    creationMetadata.KafkaTopic = nameof(Poco);

    //Act
    string statement = new CreateEntity(modelBuilder).Print<Poco>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be($@"CREATE OR REPLACE TABLE {nameof(Poco)}s (
	{nameof(Poco.Id)} INT PRIMARY KEY,
	{nameof(Poco.Description)} VARCHAR
) WITH ( KAFKA_TOPIC='{nameof(Poco)}', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings());
  }
}
