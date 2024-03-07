using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Generators;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Generators;

public class TypeGeneratorTests
{
  [Test]
  public void CreateType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator().Print(new TypeProperties<Address>());

    //Assert
    statement.Should()
      .Be($"CREATE TYPE {nameof(Address).ToUpper()} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_WithTypeName()
  {
    //Arrange
    var typeName = "MyType";

    //Act
    var statement = new TypeGenerator().Print(new TypeProperties<Address> { EntityName = typeName });

    //Assert
    statement.Should().Be($"CREATE TYPE {typeName} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_NestedType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator().Print(new TypeProperties<Person>());

    //Assert
    statement.Should().Be($"CREATE TYPE {nameof(Person).ToUpper()} AS STRUCT<Name VARCHAR, Address ADDRESS>;");
  }

  [Test]
  public void CreateType_BytesType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator().Print(new TypeProperties<Thumbnail>());

    //Assert
    statement.Should().Be($"CREATE TYPE {nameof(Thumbnail).ToUpper()} AS STRUCT<Image BYTES>;");
  }

  [Test]
  public void CreateType_MapType()
  {
    //Arrange

    //Act
    var statement = new TypeGenerator().Print(new TypeProperties<Container>());

    //Assert
    statement.Should().Be($"CREATE TYPE {nameof(Container).ToUpper()} AS STRUCT<Values2 MAP<VARCHAR, INT>>;");
  }

  [TestCase(IdentifierEscaping.Never, ExpectedResult = "CREATE TYPE ROWTIME AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords, ExpectedResult = "CREATE TYPE ROWTIME AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always, ExpectedResult = "CREATE TYPE `ROWTIME` AS STRUCT<`Value` VARCHAR>;")]
  public string CreateType_WithSystemColumName(IdentifierEscaping escaping) =>
    new TypeGenerator().Print(new TypeProperties<RowTime> { IdentifierEscaping = escaping });

  [TestCase(IdentifierEscaping.Never, ExpectedResult = "CREATE TYPE VALUES AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords, ExpectedResult = "CREATE TYPE `VALUES` AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always, ExpectedResult = "CREATE TYPE `VALUES` AS STRUCT<`Value` VARCHAR>;")]
  public string CreateType_WithReservedWord(IdentifierEscaping escaping) =>
    new TypeGenerator().Print(new TypeProperties<Values> { IdentifierEscaping = escaping });

  [TestCase(IdentifierEscaping.Never,
    ExpectedResult =
      "CREATE TYPE SYSTEMCOLUMN AS STRUCT<RowTime VARCHAR, RowOffset VARCHAR, RowPartition VARCHAR, WindowStart VARCHAR, WindowEnd VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords,
    ExpectedResult =
      "CREATE TYPE SYSTEMCOLUMN AS STRUCT<RowTime VARCHAR, RowOffset VARCHAR, RowPartition VARCHAR, WindowStart VARCHAR, WindowEnd VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always,
    ExpectedResult =
      "CREATE TYPE `SYSTEMCOLUMN` AS STRUCT<`RowTime` VARCHAR, `RowOffset` VARCHAR, `RowPartition` VARCHAR, `WindowStart` VARCHAR, `WindowEnd` VARCHAR>;")]
  public string CreateType_WithSystemColumnNameField(IdentifierEscaping escaping) =>
    new TypeGenerator().Print(new TypeProperties<SystemColumn> { IdentifierEscaping = escaping });

  public record Address
  {
    public int Number { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
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
      new TypeGenerator().Print(new TypeProperties<DatabaseChangeObject<IoTSensor>>());

    //Assert
    statement.Should()
      .Be("CREATE TYPE DATABASECHANGEOBJECT AS STRUCT<Before IOTSENSOR, After IOTSENSOR, Source SOURCE, Op VARCHAR, TsMs BIGINT>;"); //, Transaction OBJECT
  }

  #endregion
}
