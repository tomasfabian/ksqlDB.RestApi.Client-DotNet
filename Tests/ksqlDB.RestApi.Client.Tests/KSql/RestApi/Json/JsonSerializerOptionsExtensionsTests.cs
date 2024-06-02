using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using ksqlDb.RestApi.Client.KSql.RestApi.Json;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Json
{
  public class JsonSerializerOptionsExtensionsTests
  {
    [Test]
    public void WithModifier()
    {
      //Arrange
      var jsonSerializerOptions = new JsonSerializerOptions();

      void Modifier(JsonTypeInfo typeInfo)
      {
      }

      //Act
      jsonSerializerOptions.WithModifier(Modifier);

      //Assert
      jsonSerializerOptions.TypeInfoResolver.Should().NotBeNull();
      jsonSerializerOptions.TypeInfoResolver.Should().BeOfType<Client.KSql.RestApi.Json.JsonTypeInfoResolver>();
      var jsonTypeInfoResolver = jsonSerializerOptions.TypeInfoResolver as Client.KSql.RestApi.Json.JsonTypeInfoResolver;
      jsonTypeInfoResolver!.Modifiers.Should().Contain(Modifier);
      jsonTypeInfoResolver!.TypeInfoResolver.Should().BeOfType<DefaultJsonTypeInfoResolver>();
    }

    [Test]
    public void WithModifier_TypeInfoResolverContainsValue_WasDecoratedOnce()
    {
      //Arrange
      var jsonSerializerOptions = new JsonSerializerOptions()
      {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
      };

      void Modifier(JsonTypeInfo typeInfo)
      {
      }

      //Act
      jsonSerializerOptions.WithModifier(Modifier);
      jsonSerializerOptions.WithModifier(Modifier);

      //Assert
      jsonSerializerOptions.TypeInfoResolver.Should().NotBeNull();
      jsonSerializerOptions.TypeInfoResolver.Should().BeOfType<Client.KSql.RestApi.Json.JsonTypeInfoResolver>();
      var jsonTypeInfoResolver = jsonSerializerOptions.TypeInfoResolver as Client.KSql.RestApi.Json.JsonTypeInfoResolver;
      jsonTypeInfoResolver!.Modifiers.Should().Contain(Modifier);
      jsonTypeInfoResolver!.TypeInfoResolver.Should().BeEquivalentTo(jsonTypeInfoResolver.TypeInfoResolver);
    }
  }
}
