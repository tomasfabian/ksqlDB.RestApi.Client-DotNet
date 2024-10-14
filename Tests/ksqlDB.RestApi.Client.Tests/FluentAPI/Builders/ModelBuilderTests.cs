using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.FluentAPI.Builders.Configuration;
using ksqlDb.RestApi.Client.Metadata;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.FluentAPI.Builders
{
  public class ModelBuilderTests
  {
    private readonly ModelBuilder builder = new();

    [Test]
    public void Entity()
    {
      //Arrange

      //Act
      var entityBuilder = builder.Entity<Payment>();

      //Assert
      entityBuilder.Should().NotBeNull();
    }

    [Test]
    public void HasKey_Property()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>().HasKey(c => c.Id);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
    }

    [Test]
    public void HasKey_Field()
    {
      //Arrange

      //Act
      var entityTypeBuilder = builder.Entity<Bar>().HasKey(c => c.Id);

      //Assert
      entityTypeBuilder.Should().NotBeNull();
    }

    [Test]
    public void Property_Ignore()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Description)
        .Ignore()
        .Ignore();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).Ignore.Should().BeTrue();
    }

    [Test]
    public void Property_IgnoreInDML()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Description)
        .IgnoreInDML();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).IgnoreInDML.Should().BeTrue();
    }

    [Test]
    public void Property_IgnoreInDDL()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Description)
        .IgnoreInDDL();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).IgnoreInDDL.Should().BeTrue();
    }

    public class RecordExt : ksqlDB.RestApi.Client.KSql.Query.Record
    {
      public string Title { get; set; }
    }

    [Test]
    public void RowTime_PropertyConvention_IgnoreInDDL()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<RecordExt>();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(RecordExt));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(RecordExt.RowTime)).IgnoreInDDL.Should().BeTrue();
    }

    [Test]
    public void Property_HasColumnName()
    {
      //Arrange
      var columnName = "desc";

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Description)
        .HasColumnName(columnName);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).ColumnName.Should().Be(columnName);
    }

    [Test]
    public void MultiplePropertiesForSameType()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Description)
        .Ignore();

      builder.Entity<Payment>()
        .Property(b => b.Amount)
        .Ignore();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).Ignore.Should().BeTrue();
      entityMetadata.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Amount)).Ignore.Should().BeTrue();
    }

    [Test]
    public void MultipleMappingsForSameProperty()
    {
      //Arrange
      string columnName = "alter";

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Description)
        .HasColumnName(columnName);

      builder.Entity<Payment>()
        .Property(b => b.Description)
        .IgnoreInDML();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      var fieldMetadata = entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description));
      fieldMetadata.ColumnName.Should().Be(columnName);
      fieldMetadata.IgnoreInDML.Should().BeTrue();
    }

    [Test]
    public void Property_IgnoreNestedField()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Composite>()
        .Property(b => b.SubEntity.Id)
        .Ignore();

      builder.Entity<Composite>()
        .Property(b => b.Id)
        .Ignore();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Composite));
      entityMetadata.Should().NotBeNull();
      var memberInfo = GetTitleMemberInfo();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo == memberInfo).Ignore.Should().BeTrue();
    }

    [Test]
    public void AddConventionForDecimal()
    {
      //Arrange
      var decimalTypeConvention = new DecimalTypeConvention(14, 14);

      //Act
      builder.AddConvention(decimalTypeConvention);

      //Assert
      ((IMetadataProvider)builder).Conventions[typeof(decimal)].Should().BeEquivalentTo(decimalTypeConvention);
    }

    private class PaymentConfiguration : IFromItemTypeConfiguration<Payment>
    {
      public void Configure(IEntityTypeBuilder<Payment> builder)
      {
        builder.Property(b => b.Description)
          .Ignore();
      }
    }

    [Test]
    public void FromItemTypeConfiguration()
    {
      //Arrange
      var configuration = new PaymentConfiguration();

      //Act
      builder.Apply(configuration);

      //Assert
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).Ignore.Should().BeTrue();
    }

    private static MemberInfo GetTitleMemberInfo()
    {
      Expression<Func<Composite, object>> expression = foo => new { foo.Id };
      var argument = ((NewExpression)expression.Body).Arguments[0];
      var memberInfo = ((MemberExpression)argument).Member;
      return memberInfo;
    }

    [Test]
    public void Decimal_ConfigurePrecisionAndScale()
    {
      //Arrange
      short precision = 2;
      short scale = 3;

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Amount)
        .Decimal(precision, scale);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      var metadata = (DecimalFieldMetadata)entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Amount));

      metadata.Precision.Should().Be(precision);
      metadata.Scale.Should().Be(scale);
    }

    [Test]
    public void Header()
    {
      //Arrange
      string header = "abc";

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Header)
        .WithHeader(header);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      var metadata = (BytesArrayFieldMetadata)entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Header));

      metadata.Header.Should().Be(header);
    }

    [Test]
    public void Headers()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Header)
        .WithHeaders();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      var metadata = entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Header));

      metadata.HasHeaders.Should().BeTrue();
      metadata.IgnoreInDML.Should().BeTrue();
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
    public void AsStruct()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Record>()
        .Property(b => b.Headers)
        .AsStruct();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();

      var entityMetadata = ((IMetadataProvider)builder).GetEntities().FirstOrDefault(c => c.Type == typeof(Record));
      entityMetadata.Should().NotBeNull();

      var metadata = entityMetadata!.FieldsMetadata.First(c => c.IsStruct && c.Path == "Headers");
      metadata.IsStruct.Should().BeTrue();
    }
  }

  internal record Payment
  {
    public string Id { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public byte[] Header { get; set; } = null!;
  }

  internal class Bar
  {
    public string Id = null!;
  }

  internal class Composite
  {
    [JsonPropertyName("ID")]
    public string Id = null!;
    public Bar SubEntity { get; set; } = null!;
  }
}
