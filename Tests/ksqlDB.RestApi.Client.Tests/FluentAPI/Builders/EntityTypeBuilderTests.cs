using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.Metadata;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.FluentAPI.Builders
{
  public class EntityTypeBuilderTests
  {
    private EntityTypeBuilder<Tweet> builder = null!;

    [SetUp]
    public void TestInitialize()
    {
      builder = new();
    }

    [Test]
    public void HasKey()
    {
      //Arrange

      //Act
      var entityTypeBuilder = builder.HasKey(c => c.Id);

      //Assert
      entityTypeBuilder.Should().NotBeNull();
      ((EntityTypeBuilder)entityTypeBuilder).Metadata.PrimaryKeyMemberInfo.Should().NotBeNull();
    }

    [Test]
    public void Property()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Property(c => c.Id);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      builder.Metadata.FieldsMetadata.Count().Should().Be(2);
    }

    [Test]
    public void RowTime_Property_ShouldBeIgnoredInDDL()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Property(c => c.Id);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      builder.Metadata.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Tweet.RowTime)).IgnoreInDDL.Should().BeTrue();
    }

    public class Row
    {
      public long RowTime;
    }

    [Test]
    public void RowTime_Field_ShouldBeIgnoredInDDL()
    {
      //Arrange

      //Act
      EntityTypeBuilder<Row> rowBuilder = new();

      //Assert
      rowBuilder.Metadata.FieldsMetadata.First(c => c.MemberInfo.Name == nameof(Row.RowTime)).IgnoreInDDL.Should().BeTrue();
    }

    [Test]
    public void DecimalType_ShouldHaveDecimalFieldMetadata()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Property(c => c.AccountBalance);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      builder.Metadata.FieldsMetadata
        .OfType<DecimalFieldMetadata>()
        .First(c => c.MemberInfo.Name == nameof(Tweet.AccountBalance))
        .Should().NotBeNull();
    }

    public class MyValue : Row
    {
      public int Id { get; set; }
    }

    [Test]
    public void RowTimeField_AsPseudoColumn_ShouldBeRegisteredOnce()
    {
      //Arrange
      EntityTypeBuilder<MyValue> customBuilder = new();

      //Act
      customBuilder.Property(c => c.RowTime)
        .AsPseudoColumn();

      //Assert
      customBuilder.Metadata.FieldsMetadata.Count(c => c.MemberInfo.Name == nameof(Row.RowTime)).Should().Be(1);
    }
  }
}
