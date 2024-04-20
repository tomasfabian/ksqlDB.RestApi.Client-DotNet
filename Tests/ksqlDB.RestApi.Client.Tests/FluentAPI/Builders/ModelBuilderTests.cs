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
    public void Property_IgnoreField()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Payment>()
        .Property(b => b.Description)
        .Ignore()
        .Ignore();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).Ignore.Should().BeTrue();
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
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Description)).Ignore.Should().BeTrue();
      entityMetadata.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Amount)).Ignore.Should().BeTrue();
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
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Composite));
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
      builder.Conventions[typeof(decimal)].Should().BeEquivalentTo(decimalTypeConvention);
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
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
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
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
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
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Payment));
      entityMetadata.Should().NotBeNull();
      var metadata = (BytesArrayFieldMetadata)entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Payment.Header));

      metadata.Header.Should().Be(header);
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
