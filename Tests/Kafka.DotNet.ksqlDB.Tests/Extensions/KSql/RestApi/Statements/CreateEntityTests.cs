using System.Collections.Generic;
using FluentAssertions;
using Joker.Extensions;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;
using NUnit.Framework;
using Pluralize.NET;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Statements
{
  public class CreateEntityTests
  {
    private EntityCreationMetadata creationMetadata;

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

    private string CreateExpectedStatement(string creationClause, bool hasPrimaryKey, string entityName = null)
    {
      string key = hasPrimaryKey ? "PRIMARY KEY" : "KEY";

      if (entityName.IsNullOrEmpty())
        entityName = EnglishPluralizationService.Pluralize(nameof(MyMovie));

      string expectedStatementTemplate = @$"{creationClause} {entityName} (
	Id INT {key},
	Title VARCHAR,
	Release_Year INT,
	NumberOfDays ARRAY<INT>,
	Dictionary MAP<VARCHAR, INT>,
	Dictionary2 MAP<VARCHAR, INT>,
	Field DOUBLE
) WITH ( KAFKA_TOPIC='{creationMetadata.KafkaTopic}', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );";

      return expectedStatementTemplate;
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
    
    #endregion

    [Test]
    public void Print_CreateStream()
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

    private class Transaction
    {
      [Decimal(3,2)]
      public decimal Amount { get; set; }
    }

    [Test]
    public void DecimalWithPrecision()
    {
      //Arrange
      var statementContext = new StatementContext
      {
        CreationType = CreationType.Create,
        KSqlEntityType = KSqlEntityType.Stream
      };

      //Act
      string statement = new CreateEntity().Print<Transaction>(statementContext, creationMetadata, null);

      //Assert
      statement.Should().Be(@"CREATE STREAM Transactions (
	Amount DECIMAL(3,2)
) WITH ( KAFKA_TOPIC='MyMovie', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' );");
    }
    
    [Test]
    public void Print_CreateStream_OverrideEntityName()
    {
      //Arrange
      var statementContext = new StatementContext
      {
        CreationType = CreationType.Create,
        KSqlEntityType = KSqlEntityType.Stream
      };

      creationMetadata.EntityName = "TestName";

      //Act
      string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

      //Assert
      statement.Should().Be(CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, entityName: EnglishPluralizationService.Pluralize(creationMetadata.EntityName)));
    }    

    [Test]
    public void Print_CreateStream_OverrideEntityName_DonNotPluralize()
    {
      //Arrange
      var statementContext = new StatementContext
      {
        CreationType = CreationType.Create,
        KSqlEntityType = KSqlEntityType.Stream
      };

      creationMetadata.ShouldPluralizeEntityName = false;
      creationMetadata.EntityName = "TestName";

      //Act
      string statement = new CreateEntity().Print<MyMovie>(statementContext, creationMetadata, null);

      //Assert
      statement.Should().Be(CreateExpectedStatement("CREATE STREAM", hasPrimaryKey: false, entityName: creationMetadata.EntityName));
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

    public abstract record AbstractProducerClass
    {
      [Key]
      public string Key { get; set; }
    }

    record Enrichedevent1 : AbstractProducerClass
    {
      public EventCategory[] EventCategories { get; set; }
    }
    
    [Test]
    public void Print_NestedArrayType_CreateTableIfNotExists()
    {
      TestCreateEntityWithEnumerable<Enrichedevent1>();
    }

    record Enrichedevent2 : AbstractProducerClass
    {
      public List<EventCategory> EventCategories { get; set; }
    }
    
    [Test]
    public void Print_NestedListType_CreateTableIfNotExists()
    {
      TestCreateEntityWithEnumerable<Enrichedevent2>();
    }
    
    record Enrichedevent3 : AbstractProducerClass
    {
      public IEnumerable<EventCategory> EventCategories { get; set; }
    }

    [Test]
    public void Print_NestedGenericEnumerableType_CreateTableIfNotExists()
    {
      TestCreateEntityWithEnumerable<Enrichedevent3>();
    }
    
    record Enrichedevent4 : AbstractProducerClass
    {
      public int[] EventCategories { get; set; }
    }

    [Test]
    public void Print_NestedPrimitiveArrayType_CreateTableIfNotExists()
    {
      TestCreateEntityWithEnumerable<Enrichedevent4>(arrayElementType: "INT");
    }
    
    //CREATE TYPE EventCategories AS STRUCT<id INTEGER, name VARCHAR, description VARCHAR>;
    record EventCategory
    {
      public int Id { get; set; }
      public string Name { get; set; }
    }

    private static void TestCreateEntityWithEnumerable<TEntity>(string arrayElementType = "EVENTCATEGORY")
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
        Partitions = 1
      };

      //Act
      string statement = new CreateEntity().Print<TEntity>(statementContext, creationMetadata, true);

      //Assert
      statement.Should().Be(@$"CREATE TABLE IF NOT EXISTS Enrichedevents (
	EventCategories ARRAY<{arrayElementType}>,
	Key VARCHAR PRIMARY KEY
) WITH ( KAFKA_TOPIC='enrichedevents', VALUE_FORMAT='Json', PARTITIONS='1' );");
    }

    internal class MyMovie
    {
      [Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations.Key]
      public int Id { get; set; }

      public string Title { get; set; }

      public int Release_Year { get; set; }

      public int[] NumberOfDays { get; set; }

      public IDictionary<string, int> Dictionary { get; set; }
      public Dictionary<string, int> Dictionary2 { get; set; }

      public double Field;

      public int DontFindMe { get; }
    }
  }
}