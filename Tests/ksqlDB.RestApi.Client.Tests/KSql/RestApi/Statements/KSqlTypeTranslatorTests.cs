using System.Drawing;
using System.Text;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements
{
  public class KSqlTypeTranslatorTests
  {
    private ModelBuilder modelBuilder = null!;
    private KSqlTypeTranslator kSqlTypeTranslator = null!;

    [SetUp]
    public void Init()
    {
      modelBuilder = new();
      kSqlTypeTranslator = new(modelBuilder);
    }

    [Test]
    public void Translate_StringType()
    {
      //Arrange
      var type = typeof(string);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Varchar);
    }

    [Test]
    public void Translate_IntType()
    {
      //Arrange
      var type = typeof(int);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Int);
    }

    [Test]
    public void Translate_LongType()
    {
      //Arrange
      var type = typeof(long);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.BigInt);
    }

    [Test]
    public void Translate_DoubleType()
    {
      //Arrange
      var type = typeof(double);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Double);
    }

    [Test]
    public void Translate_DecimalType()
    {
      //Arrange
      var type = typeof(decimal);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Decimal);
    }

    [Test]
    public void Translate_BoolType()
    {
      //Arrange
      var type = typeof(bool);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Boolean);
    }

    [Test]
    public void Translate_DictionaryType()
    {
      //Arrange
      var type = typeof(Dictionary<string, int>);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Int}>");
    }

    [Test]
    public void Translate_DictionaryInterface()
    {
      //Arrange
      var type = typeof(IDictionary<string, long>);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.BigInt}>");
    }

    [Test]
    public void Translate_ArrayType()
    {
      //Arrange
      var type = typeof(double[]);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Array}<{KSqlTypes.Double}>");
    }

    [Test]
    public void Translate_EnumerableType()
    {
      //Arrange
      var type = typeof(IEnumerable<string>);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Array}<{KSqlTypes.Varchar}>");
    }

    [Test]
    public void Translate_ListType()
    {
      //Arrange
      var type = typeof(List<string>);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Array}<{KSqlTypes.Varchar}>");
    }

    [Test]
    public void Translate_IListInterface()
    {
      //Arrange
      var type = typeof(IList<string>);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Array}<{KSqlTypes.Varchar}>");
    }

    [Test]
    public void Translate_IEnumerable()
    {
      //Arrange
      var type = typeof(IEnumerable<string>);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Array}<{KSqlTypes.Varchar}>");
    }

    [Test]
    public void Translate_NestedMapInArray()
    {
      //Arrange
      var type = typeof(IDictionary<string, int>[]);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Array}<{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Int}>>");
    }

    [Test]
    public void Translate_NestedArrayInMap()
    {
      //Arrange
      var type = typeof(IDictionary<string, int[]>);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Array}<{KSqlTypes.Int}>>");
    }

    [Test]
    public void Translate_BytesType()
    {
      //Arrange
      var type = typeof(byte[]);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Bytes);
    }

    [Test]
    public void Translate_GuidType()
    {
      //Arrange
      var type = typeof(Guid);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Varchar);
    }

    [Test]
    public void Translate_DateTimeType()
    {
      //Arrange
      var type = typeof(DateTime);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Date);
    }

    [Test]
    public void Translate_TimeSpanType()
    {
      //Arrange
      var type = typeof(TimeSpan);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Time);
    }

    [Test]
    public void Translate_DateTimeOffsetType()
    {
      //Arrange
      var type = typeof(DateTimeOffset);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Timestamp);
    }

    [Test]
    public void Translate_EnumType()
    {
      //Arrange
      var type = typeof(ConsoleColor);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(KSqlTypes.Varchar);
    }

    [Test]
    public void Translate_ClassType()
    {
      //Arrange
      var type = typeof(StringBuilder);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(nameof(StringBuilder).ToUpper());
    }

    [Test]
    public void Translate_StructType()
    {
      //Arrange
      var type = typeof(Point);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be(nameof(Point).ToUpper());
    }

    [Struct]
    private class Decorated
    {
      public required int Foo { get; set; }
      public required string Bzr { get; set; }
    }

    [Test]
    public void Translate_StructAttributeType()
    {
      //Arrange
      var type = typeof(Decorated);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Struct}<{nameof(Decorated.Foo)} {KSqlTypes.Int}, {nameof(Decorated.Bzr)} {KSqlTypes.Varchar}>");
    }

    [Struct]
    private record IoTSensor
    {
      [Headers("abc")]
      public byte[] Header { get; set; } = null!;
    }

    [Test]
    public void Translate_HeadersAttributeType()
    {
      //Arrange
      var type = typeof(IoTSensor);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Struct}<{nameof(IoTSensor.Header)} {KSqlTypes.Bytes} HEADER('abc')>");
    }

    [Struct]
    private record Account
    {
      [Decimal(10,4)]
      public decimal Amount { get; set; }
    }

    [Test]
    public void Translate_DecimalAttributeType()
    {
      //Arrange
      var type = typeof(Account);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Struct}<{nameof(Account.Amount)} {KSqlTypes.Decimal}(10,4)>");
    }

    [Struct]
    private record Poco
    {
      public decimal Amount { get; set; }
    }

    [Test]
    public void Translate_UseModelBuilderConfiguration()
    {
      //Arrange
      var type = typeof(Poco);
      modelBuilder.Entity<Poco>()
        .Property(c => c.Amount)
        .Decimal(10, 2);

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Struct}<{nameof(Poco.Amount)} {KSqlTypes.Decimal}(10,2)>");
    }

    [Test]
    public void Translate_UseModelBuilderConvention()
    {
      //Arrange
      var type = typeof(Poco);
      modelBuilder.AddConvention(new DecimalTypeConvention(10, 3));

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Struct}<{nameof(Poco.Amount)} {KSqlTypes.Decimal}(10,3)>");
    }
  }
}
