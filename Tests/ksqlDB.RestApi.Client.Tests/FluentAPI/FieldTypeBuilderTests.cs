using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.Metadata;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.FluentAPI
{
  public class FieldTypeBuilderTests
  {
    private FieldMetadata fieldMetadata = null!;
    private FieldTypeBuilder<Tweet> builder = null!;

    [SetUp]
    public void TestInitialize()
    {
      fieldMetadata = new();
      builder = new(fieldMetadata);
    }

    [Test]
    public void InitState()
    {
      //Arrange

      //Act

      //Assert
      fieldMetadata.Ignore.Should().BeFalse();
      fieldMetadata.IgnoreInDDL.Should().BeFalse();
      fieldMetadata.IgnoreInDML.Should().BeFalse();

      fieldMetadata.HasHeaders.Should().BeFalse();
      fieldMetadata.IsStruct.Should().BeFalse();
      fieldMetadata.ColumnName.Should().BeNull();
    }

    [Test]
    public void Ignore()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.Ignore();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      fieldMetadata.Ignore.Should().BeTrue();
      fieldMetadata.IgnoreInDDL.Should().BeFalse();
      fieldMetadata.IgnoreInDML.Should().BeFalse();
    }

    [Test]
    public void IgnoreInDDL()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.IgnoreInDDL();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      fieldMetadata.Ignore.Should().BeFalse();
      fieldMetadata.IgnoreInDDL.Should().BeTrue();
      fieldMetadata.IgnoreInDML.Should().BeFalse();
    }

    [Test]
    public void IgnoreInDML()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.IgnoreInDML();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      fieldMetadata.Ignore.Should().BeFalse();
      fieldMetadata.IgnoreInDDL.Should().BeFalse();
      fieldMetadata.IgnoreInDML.Should().BeTrue();
    }

    [Test]
    public void AsStruct()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.AsStruct();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      fieldMetadata.IsStruct.Should().BeTrue();
    }

    [Test]
    public void HasColumnName()
    {
      //Arrange
      string columnName = "alter";

      //Act
      var fieldTypeBuilder = builder.HasColumnName(columnName);

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      fieldMetadata.ColumnName.Should().Be(columnName);
    }

    [Test]
    public void WithHeaders()
    {
      //Arrange

      //Act
      var fieldTypeBuilder = builder.WithHeaders();

      //Assert
      fieldTypeBuilder.Should().NotBeNull();
      fieldMetadata.HasHeaders.Should().BeTrue();
    }
  }
}
