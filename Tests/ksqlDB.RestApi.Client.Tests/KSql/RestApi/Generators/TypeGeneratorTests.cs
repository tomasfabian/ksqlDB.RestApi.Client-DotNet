using FluentAssertions;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
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
    string statement = new TypeGenerator().Print<Address>(new TypeProperties
      { EntityName = typeof(Address).ExtractTypeName().ToUpper() });

    //Assert
    statement.Should()
      .Be($@"CREATE TYPE {nameof(Address).ToUpper()} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_WithTypeName()
  {
    //Arrange
    string typeName = "MyType";

    //Act
    string statement = new TypeGenerator().Print<Address>(new TypeProperties { EntityName = typeName });

    //Assert
    statement.Should().Be($@"CREATE TYPE {typeName} AS STRUCT<Number INT, Street VARCHAR, City VARCHAR>;");
  }

  [Test]
  public void CreateType_NestedType()
  {
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<Person>(new TypeProperties
      { EntityName = typeof(Person).ExtractTypeName().ToUpper() });

    //Assert
    statement.Should().Be($@"CREATE TYPE {nameof(Person).ToUpper()} AS STRUCT<Name VARCHAR, Address ADDRESS>;");
  }

  [Test]
  public void CreateType_BytesType()
  {
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<Thumbnail>(new TypeProperties
      { EntityName = typeof(Thumbnail).ExtractTypeName().ToUpper() });

    //Assert
    statement.Should().Be(@$"CREATE TYPE {nameof(Thumbnail).ToUpper()} AS STRUCT<Image BYTES>;");
  }

  [Test]
  public void CreateType_MapType()
  {
    //Arrange

    //Act
    string statement = new TypeGenerator().Print<Container>(new TypeProperties
      { EntityName = typeof(Container).ExtractTypeName().ToUpper() });

    //Assert
    statement.Should().Be(@$"CREATE TYPE {nameof(Container).ToUpper()} AS STRUCT<Values2 MAP<VARCHAR, INT>>;");
  }

  [TestCase(IdentifierEscaping.Never, ExpectedResult = "CREATE TYPE ROWTIME AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords, ExpectedResult = "CREATE TYPE ROWTIME AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always, ExpectedResult = "CREATE TYPE `ROWTIME` AS STRUCT<`Value` VARCHAR>;")]
  public string CreateType_WithSystemColumName(IdentifierEscaping escaping) =>
    new TypeGenerator().Print<Rowtime>(new TypeProperties
      { EntityName = typeof(Rowtime).ExtractTypeName().ToUpper(), IdentifierEscaping = escaping });

  [TestCase(IdentifierEscaping.Never, ExpectedResult = "CREATE TYPE VALUES AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords, ExpectedResult = "CREATE TYPE `VALUES` AS STRUCT<Value VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always, ExpectedResult = "CREATE TYPE `VALUES` AS STRUCT<`Value` VARCHAR>;")]
  public string CreateType_WithReservedWord(IdentifierEscaping escaping) =>
    new TypeGenerator().Print<Values>(new TypeProperties
      { EntityName = typeof(Values).ExtractTypeName().ToUpper(), IdentifierEscaping = escaping });

  [TestCase(IdentifierEscaping.Never,
    ExpectedResult =
      "CREATE TYPE SYSTEMCOLUMN AS STRUCT<Rowtime VARCHAR, Rowoffset VARCHAR, Rowpartition VARCHAR, Windowstart VARCHAR, Windowend VARCHAR>;")]
  [TestCase(IdentifierEscaping.Keywords,
    ExpectedResult =
      "CREATE TYPE SYSTEMCOLUMN AS STRUCT<Rowtime VARCHAR, Rowoffset VARCHAR, Rowpartition VARCHAR, Windowstart VARCHAR, Windowend VARCHAR>;")]
  [TestCase(IdentifierEscaping.Always,
    ExpectedResult =
      "CREATE TYPE `SYSTEMCOLUMN` AS STRUCT<`Rowtime` VARCHAR, `Rowoffset` VARCHAR, `Rowpartition` VARCHAR, `Windowstart` VARCHAR, `Windowend` VARCHAR>;")]
  public string CreateType_WithSystemColumnNameField(IdentifierEscaping escaping) =>
    new TypeGenerator().Print<SystemColumn>(new TypeProperties { EntityName = typeof(SystemColumn).ExtractTypeName().ToUpper(), IdentifierEscaping = escaping });

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

  record Rowtime(string Value)
  {
  }

  record Values(string Value)
  {
  }

  record SystemColumn(string Rowtime, string Rowoffset, string Rowpartition, string Windowstart, string Windowend)
  {
  }

  #region GenericType

  record DatabaseChangeObject<TEntity> : DatabaseChangeObject
  {
    public TEntity Before { get; set; } = default!;
    public TEntity After { get; set; } = default!;
  }

  record DatabaseChangeObject
  {
    public Source Source { get; set; } = null!;
    public string Op { get; set; } = null!;

    public long TsMs { get; set; }

    public ChangeDataCaptureType OperationType => ChangeDataCaptureType.Created;
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
    string statement = new TypeGenerator().Print<DatabaseChangeObject<IoTSensor>>(new TypeProperties { EntityName = typeof(DatabaseChangeObject<IoTSensor>).ExtractTypeName().ToUpper()});

    //Assert
    statement.Should()
      .Be(
        @"CREATE TYPE DATABASECHANGEOBJECT AS STRUCT<Before IOTSENSOR, After IOTSENSOR, Source SOURCE, Op VARCHAR, TsMs BIGINT>;"); //, Transaction OBJECT
  }

  #endregion
}
