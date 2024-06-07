using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FluentAssertions;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;
using UnitTests;
using JsonTypeInfoResolver = ksqlDb.RestApi.Client.KSql.RestApi.Json.JsonTypeInfoResolver;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Json
{
  public class JsonTypeInfoResolverTests : TestBase
  {
    [Test]
    public void GetTypeInfo()
    {
      //Arrange
      var resolver = new JsonTypeInfoResolver(new DefaultJsonTypeInfoResolver())
      {
        Modifiers = { ToUpperPropertyNameModifier }
      };

      //Act
      var typeInfo = resolver.GetTypeInfo(typeof(Tweet), new JsonSerializerOptions());

      //Assert
      typeInfo!.Type.Should().Be(typeof(Tweet));
      typeInfo!.Type.Properties().Count().Should().Be(5);
      typeInfo.Properties.Any(c => c.Name == nameof(Tweet.Amount).ToUpper()).Should().BeTrue();
    }

    internal void ToUpperPropertyNameModifier(JsonTypeInfo jsonTypeInfo)
    {
      foreach (var jsonPropertyInfo in jsonTypeInfo.Properties)
      {
        jsonPropertyInfo.Name = jsonPropertyInfo.Name.ToUpper();
      }
    }
  }
}
