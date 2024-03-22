using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDb.RestApi.Client.Metadata;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.Metadata
{
  public class ModelBuilderTests
  {
    private readonly ModelBuilder builder = new();

    [Test]
    public void Entity()
    {
      //Arrange

      //Act
      var entityBuilder = builder.Entity<Foo>();

      //Assert
      entityBuilder.Should().NotBeNull();
    }

    [Test]
    public void HasKey_Property()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Entity<Foo>().HasKey(c => c.Id);

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
      var fieldTypeBuilder = builder.Entity<Foo>()
        .Property(b => b.Title)
        .Ignore()
        .Ignore();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Foo));
      entityMetadata.Should().NotBeNull();
      entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Foo.Title)).Ignore.Should().BeTrue();
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

    private static MemberInfo GetTitleMemberInfo()
    {
      Expression<Func<Composite, object>> expression = foo => new { foo.Id };
      var argument = ((NewExpression)expression.Body).Arguments[0];
      var memberInfo = ((MemberExpression)argument).Member;
      return memberInfo;
    }

    [Test]
    public void Decimal_Precision()
    {
      //Arrange
      short precision = 2;
      short scale = 3;

      //Act
      var fieldTypeBuilder = builder.Entity<Foo>()
        .Property(b => b.Amount)
        .Precision(precision, scale);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      var entityMetadata = builder.GetEntities().FirstOrDefault(c => c.Type == typeof(Foo));
      entityMetadata.Should().NotBeNull();
      var metadata = (DecimalFieldMetadata) entityMetadata!.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Foo.Amount));

      metadata.Precision.Should().Be(precision);
      metadata.Scale.Should().Be(scale);
    }
  }

  internal record Foo
  {
    public string Id { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Title { get; set; } = null!;
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
