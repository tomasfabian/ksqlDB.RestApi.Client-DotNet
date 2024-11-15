using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDb.RestApi.Client.Infrastructure.Attributes;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.Infrastructure.Attributes
{
  enum TestEnum
  {
    A,
    B
  }

  public class JsonSnakeCaseStringEnumConverterAttributeTests
  {
    [Test]
    public void CreatesConverter()
    {
      // Arrange
      var attr = new JsonSnakeCaseStringEnumConverterAttribute<TestEnum>();

      // Act
      var converter = attr.CreateConverter(typeof(TestEnum));

      // Assert
      converter.Should().BeOfType<JsonStringEnumConverter<TestEnum>>();
    }
  }
}
