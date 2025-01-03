using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Generators;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Generators;

public class TypeGeneratorTests
{
  private ModelBuilder modelBuilder;

  [SetUp]
  public void TestInitialize()
  {
    modelBuilder = new();
  }

  [Test]
  public void CreateType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Address>(new TypeProperties());

    //Assert
    statement.Should()
      .Be($"CREATE TYPE {nameof(Address)} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  private record Test
  {
    [JsonPropertyName("Id")]
    public int Override { get; set; }
  }

  [Test]
  public void CreateType_JsonPropertyName()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Test>(new TypeProperties());

    //Assert
    statement.Should()
      .Be($"CREATE TYPE {nameof(Test)} AS STRUCT<Id INT>;");
  }

  [Test]
  public void CreateType_ModelBuilder_HasColumnName()
  {
    //Arrange
    string columnName = "No";
    modelBuilder.Entity<Address>()
      .Property(b => b.Number)
      .HasColumnName(columnName);

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Address>(new TypeProperties());

    //Assert
    statement.Should()
      .Be($"CREATE TYPE {nameof(Address)} AS STRUCT<{columnName} INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_ModelBuilder_IgnoreInDDL()
  {
    //Arrange
    modelBuilder.Entity<Address>()
      .Property(b => b.Number)
      .IgnoreInDDL();

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Address>(new TypeProperties());

    //Assert
    statement.Should()
      .Be($"CREATE TYPE {nameof(Address)} AS STRUCT<Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_WithTypeName()
  {
    //Arrange
    var typeName = "MYTYPE";

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Address>(new TypeProperties { EntityName = typeName });

    //Assert
    statement.Should().Be($"CREATE TYPE {typeName} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_NestedType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Person>(new TypeProperties());

    //Assert
    statement.Should().Be($"CREATE TYPE {nameof(Person)} AS STRUCT<Name VARCHAR, Address ADDRESS>;");
  }

  [Test]
  public void CreateType_BytesType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Thumbnail>(new TypeProperties());

    //Assert
    statement.Should().Be($"CREATE TYPE {nameof(Thumbnail)} AS STRUCT<Image BYTES>;");
  }

  [Test]
  public void CreateType_MapType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator(modelBuilder).Print<Container>(new TypeProperties());

    //Assert
    statement.Should().Be($"CREATE TYPE {nameof(Container)} AS STRUCT<Values2 MAP<VARCHAR, INT>>;");
  }

  [TestCase(IdentifierEscaping.Never, ExpectedResult = "CREATE TYPE RowTime AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords, ExpectedResult = "CREATE TYPE RowTime AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always, ExpectedResult = "CREATE TYPE `RowTime` AS STRUCT<`Value` VARCHAR>;")]
  public string CreateType_WithSystemColumName(IdentifierEscaping escaping) =>
    new TypeGenerator(modelBuilder).Print<RowTime>(new TypeProperties { IdentifierEscaping = escaping });

  [TestCase(IdentifierEscaping.Never, ExpectedResult = "CREATE TYPE Values AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords, ExpectedResult = "CREATE TYPE `Values` AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always, ExpectedResult = "CREATE TYPE `Values` AS STRUCT<`Value` VARCHAR>;")]
  public string CreateType_WithReservedWord(IdentifierEscaping escaping) =>
    new TypeGenerator(modelBuilder).Print<Values>(new TypeProperties { IdentifierEscaping = escaping });

  [TestCase(IdentifierEscaping.Never,
    ExpectedResult =
      "CREATE TYPE SystemColumn AS STRUCT<RowTime VARCHAR, RowOffset VARCHAR, RowPartition VARCHAR, WindowStart VARCHAR, WindowEnd VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords,
    ExpectedResult =
      "CREATE TYPE SystemColumn AS STRUCT<RowTime VARCHAR, RowOffset VARCHAR, RowPartition VARCHAR, WindowStart VARCHAR, WindowEnd VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always,
    ExpectedResult =
      "CREATE TYPE `SystemColumn` AS STRUCT<`RowTime` VARCHAR, `RowOffset` VARCHAR, `RowPartition` VARCHAR, `WindowStart` VARCHAR, `WindowEnd` VARCHAR>;")]
  public string CreateType_WithSystemColumnNameField(IdentifierEscaping escaping) =>
    new TypeGenerator(modelBuilder).Print<SystemColumn>(new TypeProperties { IdentifierEscaping = escaping });

  public record Address
  {
    public int Number { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    [IgnoreInDDL]
    public string Secret { get; set; } = null!;
  }

  public class Person
  {
    public string Name { get; set; } = null!;
    public Address Address { get; set; } = null!;
  }

  public struct Thumbnail
  {
    public byte[] Image { get; set; }
  }

  record Container
  {
    public IDictionary<string, int> Values2 { get; set; } = null!;
  }

  private record RowTime(string Value)
  {
  }

  private record Values(string Value)
  {
  }

  private record SystemColumn(string RowTime, string RowOffset, string RowPartition, string WindowStart, string WindowEnd)
  {
  }

  #region GenericType

  private record DatabaseChangeObject<TEntity> : DatabaseChangeObject
  {
    public TEntity Before { get; set; } = default!;
    public TEntity After { get; set; } = default!;
  }

  record DatabaseChangeObject
  {
    public Source Source { get; set; } = null!;
    public string Op { get; set; } = null!;

    public long TsMs { get; set; }

    public static ChangeDataCaptureType OperationType => ChangeDataCaptureType.Created;
  }

  [Flags]
  enum ChangeDataCaptureType
  {
    Read,
    Created,
    Updated,
    Deleted
  }

  record IoTSensor
  {
    [Key] public string SensorId { get; set; } = null!;
    public int Value { get; set; }
  }

  public record Source
  {
    public string Version { get; set; } = null!;
    public string Connector { get; set; } = null!;
  }

  [Test]
  public void CreateType_GenericType()
  {
    //Arrange

    //Act
    var statement =
      new TypeGenerator(modelBuilder).Print<DatabaseChangeObject<IoTSensor>>(new TypeProperties());

    //Assert
    statement.Should()
      .Be($"CREATE TYPE {nameof(DatabaseChangeObject)} AS STRUCT<Before IOTSENSOR, After IOTSENSOR, Source SOURCE, Op VARCHAR, TsMs BIGINT>;"); //, Transaction OBJECT
  }

  #endregion
}
