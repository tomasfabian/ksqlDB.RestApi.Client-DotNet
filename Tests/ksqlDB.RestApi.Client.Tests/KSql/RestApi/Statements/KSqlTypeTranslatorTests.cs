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
    private KSqlTypeTranslator<Poco> kSqlTypeTranslator = null!;

    [SetUp]
    public void Init()
    {
      modelBuilder = new();
      kSqlTypeTranslator = new(modelBuilder);
    }

    [TestCase(typeof(string), KSqlTypes.Varchar)]
    [TestCase(typeof(short), KSqlTypes.Int)]
    [TestCase(typeof(int), KSqlTypes.Int)]
    [TestCase(typeof(long), KSqlTypes.BigInt)]
    [TestCase(typeof(double), KSqlTypes.Double)]
    [TestCase(typeof(decimal), KSqlTypes.Decimal)]
    [TestCase(typeof(bool), KSqlTypes.Boolean)]
    [TestCase(typeof(short?), KSqlTypes.Int)]
    [TestCase(typeof(int?), KSqlTypes.Int)]
    [TestCase(typeof(long?), KSqlTypes.BigInt)]
    [TestCase(typeof(double?), KSqlTypes.Double)]
    [TestCase(typeof(decimal?), KSqlTypes.Decimal)]
    [TestCase(typeof(bool?), KSqlTypes.Boolean)]
    [TestCase(typeof(Dictionary<string, int>), $"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Int}>")]
    [TestCase(typeof(Dictionary<string, int?>), $"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Int}>")]
    [TestCase(typeof(IDictionary<string, long>), $"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.BigInt}>")]
    [TestCase(typeof(IDictionary<string, long?>), $"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.BigInt}>")]
    [TestCase(typeof(double[]), $"{KSqlTypes.Array}<{KSqlTypes.Double}>")]
    [TestCase(typeof(IEnumerable<string>), $"{KSqlTypes.Array}<{KSqlTypes.Varchar}>")]
    [TestCase(typeof(List<string>), $"{KSqlTypes.Array}<{KSqlTypes.Varchar}>")]
    [TestCase(typeof(IList<string>), $"{KSqlTypes.Array}<{KSqlTypes.Varchar}>")]
    [TestCase(typeof(IDictionary<string, int>[]), $"{KSqlTypes.Array}<{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Int}>>")]
    [TestCase(typeof(IDictionary<string, int?>[]), $"{KSqlTypes.Array}<{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Int}>>")]
    [TestCase(typeof(IDictionary<string, int[]>), $"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Array}<{KSqlTypes.Int}>>")]
    [TestCase(typeof(IDictionary<string, int[]?>), $"{KSqlTypes.Map}<{KSqlTypes.Varchar}, {KSqlTypes.Array}<{KSqlTypes.Int}>>")]
    [TestCase(typeof(byte[]), KSqlTypes.Bytes)]
    [TestCase(typeof(Guid), KSqlTypes.Varchar)]
    [TestCase(typeof(Guid?), KSqlTypes.Varchar)]
    [TestCase(typeof(DateTime), KSqlTypes.Date)]
    [TestCase(typeof(DateTime?), KSqlTypes.Date)]
    [TestCase(typeof(TimeSpan), KSqlTypes.Time)]
    [TestCase(typeof(TimeSpan?), KSqlTypes.Time)]
    [TestCase(typeof(DateTimeOffset), KSqlTypes.Timestamp)]
    [TestCase(typeof(DateTimeOffset?), KSqlTypes.Timestamp)]
    [TestCase(typeof(ConsoleColor), KSqlTypes.Varchar)]
    [TestCase(typeof(ConsoleColor?), KSqlTypes.Varchar)]
    [TestCase(typeof(Account), $"{KSqlTypes.Struct}<{nameof(Account.Amount)} {KSqlTypes.Decimal}(10,4)>")]
    public void Translate_TypeToKsqlType(Type type, string expectedKsqlType)
    {
        // Act
        string ksqlType = kSqlTypeTranslator.Translate(type);

        // Assert
        ksqlType.Should().Be(expectedKsqlType);
    }

    [TestCase(typeof(StringBuilder), nameof(StringBuilder))]
    [TestCase(typeof(Point), nameof(Point))]
    public void Translate_TypeToKsqlType_2(Type type, string expectedKsqlType)
    {
        // Act
        string ksqlType = kSqlTypeTranslator.Translate(type);

        // Assert
        ksqlType.Should().Be(expectedKsqlType.ToUpperInvariant());
    }
    [Struct]
    private record Account
    {
      [Decimal(10,4)]
      public decimal Amount { get; set; }
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
      string ksqlType = kSqlTypeTranslator.Translate(type, type.GetMember(nameof(Poco.Amount))[0]);

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

    [Struct]
    private record PocoEx : Poco
    {
      public string Description { get; init; } = null!;
    }

    [Test]
    public void Translate_UseModelBuilderConfiguration_Ignore()
    {
      //Arrange
      var kSqlTypeTranslator = new KSqlTypeTranslator<PocoEx>(modelBuilder);
      var type = typeof(PocoEx);
      modelBuilder.Entity<PocoEx>()
        .Property(c => c.Description)
        .Ignore();

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Struct}<{nameof(PocoEx.Amount)} {KSqlTypes.Decimal}>");
    }

    [Test]
    public void Translate_UseModelBuilderConfiguration_IgnorePropertyFromBaseType()
    {
      //Arrange
      var kSqlTypeTranslator  = new KSqlTypeTranslator<PocoEx>(modelBuilder);
      var type = typeof(PocoEx);
      modelBuilder.Entity<PocoEx>()
        .Property(c => c.Amount)
        .Ignore();

      //Act
      string ksqlType = kSqlTypeTranslator.Translate(type);

      //Assert
      ksqlType.Should().Be($"{KSqlTypes.Struct}<{nameof(PocoEx.Description)} {KSqlTypes.Varchar}>");
    }
  }
}
