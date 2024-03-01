using System.Collections;
using System.Text.Json.Serialization;
using FluentAssertions;
using Joker.Extensions;
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

  private static readonly IPluralize EnglishPluralizationService = new Pluralizer();

  private static string CreateExpectedStatement(string creationClause, bool hasPrimaryKey, string? entityName = null, IdentifierEscaping escaping = Never)
  {
    string key = hasPrimaryKey ? "PRIMARY KEY" : "KEY";

    if (entityName.IsNullOrEmpty())
      entityName = EnglishPluralizationService.Pluralize(nameof(MyMovie));

    switch (escaping)
    {
      case Never:
      case Keywords:
        return @$"{creationClause} {entityName} (
	Id INT {key},
	Title VARCHAR,
	Release_Year INT,
	NumberOfDays ARRAY<INT>,
	Dictionary MAP<VARCHAR, INT>,
	Dictionary2 MAP<VARCHAR, INT>,
	Field DOUBLE
) WITH ( KAFKA_TOPIC='{nameof(MyMovie)}', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings();
        break;
      case Always:
        return @$"{creationClause} `{entityName}` (
	`Id` INT {key},
	`Title` VARCHAR,
	`Release_Year` INT,
	`NumberOfDays` ARRAY<INT>,
	`Dictionary` MAP<VARCHAR, INT>,
	`Dictionary2` MAP<VARCHAR, INT>,
	`Field` DOUBLE
) WITH ( KAFKA_TOPIC='{nameof(MyMovie)}', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );".ReplaceLineEndings();
      default:
        throw new ArgumentOutOfRangeException(nameof(escaping), escaping, "Non-exhaustive match");
    }
  }

  #region KSqlTypeTranslator

  [Test]
  public void KSqlTypeTranslator_StringType()
  {
    //Arrange
    var type = typeof(string);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("VARCHAR");
  }

  [Test]
  public void KSqlTypeTranslator_IntType()
  {
    //Arrange
    var type = typeof(int);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("INT");
  }

  [Test]
  public void KSqlTypeTranslator_LongType()
  {
    //Arrange
    var type = typeof(long);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("BIGINT");
  }

  [Test]
  public void KSqlTypeTranslator_DoubleType()
  {
    //Arrange
    var type = typeof(double);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("DOUBLE");
  }

  [Test]
  public void KSqlTypeTranslator_DecimalType()
  {
    //Arrange
    var type = typeof(decimal);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("DECIMAL");
  }

  [Test]
  public void KSqlTypeTranslator_BoolType()
  {
    //Arrange
    var type = typeof(bool);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("BOOLEAN");
  }

  [Test]
  public void KSqlTypeTranslator_DictionaryType()
  {
    //Arrange
    var type = typeof(Dictionary<string, int>);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("MAP<VARCHAR, INT>");
  }

  [Test]
  public void KSqlTypeTranslator_DictionaryInterface()
  {
    //Arrange
    var type = typeof(IDictionary<string, long>);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("MAP<VARCHAR, BIGINT>");
  }

  [Test]
  public void KSqlTypeTranslator_ArrayType()
  {
    //Arrange
    var type = typeof(double[]);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("ARRAY<DOUBLE>");
  }

  [Test]
  public void KSqlTypeTranslator_ListType()
  {
    //Arrange
    var type = typeof(List<string>);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("ARRAY<VARCHAR>");
  }

  [Test]
  public void KSqlTypeTranslator_IListInterface()
  {
    //Arrange
    var type = typeof(IList<string>);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("ARRAY<VARCHAR>");
  }

  [Test]
  public void KSqlTypeTranslator_IEnumerable()
  {
    //Arrange
    var type = typeof(IEnumerable<string>);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("ARRAY<VARCHAR>");
  }

  [Test]
  public void KSqlTypeTranslator_NestedMapInArray()
  {
    //Arrange
    var type = typeof(IDictionary<string, int>[]);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("ARRAY<MAP<VARCHAR, INT>>");
  }

  [Test]
  public void KSqlTypeTranslator_NestedArrayInMap()
  {
    //Arrange
    var type = typeof(IDictionary<string, int[]>);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("MAP<VARCHAR, ARRAY<INT>>");
  }

  [Test]
  public void KSqlTypeTranslator_BytesType()
  {
    //Arrange
    var type = typeof(byte[]);

    //Act
    string ksqlType = CreateEntity.KSqlTypeTranslator(type);

    //Assert
    ksqlType.Should().Be("BYTES");
  }

  #endregion


  public static IEnumerable<(IdentifierEscaping, string)> PrintCreateStreamTestCases()
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
    var statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  private class Transaction
  {
    [Decimal(3,2)]
    public decimal Amount { get; set; }
  }

  public static IEnumerable<(IdentifierEscaping, string)> DecimalWithPrecisionTestCases()
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
    var statement = new CreateEntity().Print<Transaction>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  public static IEnumerable<(IdentifierEscaping, string)> PrintCreatStreamOverrideEntityNameTestCases()
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
    var statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(expected);
  }

  public static IEnumerable<(IdentifierEscaping, string)> PrintCreateStreamOverrideEntityNameDoNotPluralizeTestCases()
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
    var statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

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
    string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

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
    string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, ifNotExists: true);

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
    string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

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
    string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

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
    string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, ifNotExists: true);

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
    string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

    //Assert
    statement.Should().Be(CreateExpectedStatement("CREATE OR REPLACE TABLE", hasPrimaryKey: true));
  }

  public static IEnumerable<(IdentifierEscaping, string)> PrintCreateOrReplaceTableIncludeReadOnlyPropertiesTestCases()
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
    var statement = new CreateEntity().Print<MyItems>(statementContext, creationMetadata, null);

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

  private static void TestCreateEntityWithEnumerable<TEntity>(string arrayElementType = "EVENTCATEGORY", IdentifierEscaping escaping = Never)
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
    var statement = new CreateEntity().Print<TEntity>(statementContext, creationMetadata, true);

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

    public IEnumerable<int> Items { get; } = new List<int>();
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
    string statement = new CreateEntity().Print<TimeTypes>(statementContext, streamCreationMetadata, false);

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
    string statement = new CreateEntity().Print<Renamed>(statementContext, streamCreationMetadata, false);

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
    string statement = new CreateEntity().Print<GuidKey>(statementContext, streamCreationMetadata, false);

    //Assert
    statement.Should().Be(@$"CREATE STREAM {nameof(GuidKey)} (
	DataId VARCHAR
) WITH ( KAFKA_TOPIC='{streamCreationMetadata.KafkaTopic}', VALUE_FORMAT='Json', PARTITIONS='1' );".ReplaceLineEndings());
  }
}
