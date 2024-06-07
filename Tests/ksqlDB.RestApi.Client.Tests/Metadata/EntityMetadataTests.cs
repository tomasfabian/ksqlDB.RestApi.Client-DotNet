using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDb.RestApi.Client.Metadata;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.Metadata
{
  public class EntityMetadataTests
  {
    private EntityMetadata entityMetadata = null!;

    [SetUp]
    public void TestInitialize()
    {
      entityMetadata = new();
    }

    [Test]
    public void Add_MemberWasStored()
    {
      //Arrange
      var memberExpression = GetTitleMemberExpression();

      //Act
      var added = entityMetadata.Add(memberExpression);

      //Assert
      added.Should().BeTrue();
    }

    private static MemberExpression GetTitleMemberExpression()
    {
      Expression<Func<Foo, object>> expression = foo => new { foo.Title };
      var argument = ((NewExpression)expression.Body).Arguments[0];
      return (MemberExpression)argument;
    }

    [Test]
    public void Add_MemberWasNotStoredSecondTime()
    {
      //Arrange
      var memberExpression = GetTitleMemberExpression();
      entityMetadata.Add(memberExpression);

      //Act
      var added = entityMetadata.Add(GetTitleMemberExpression());

      //Assert
      added.Should().BeFalse();
    }

    [Test]
    public void TryGetMemberExpression_UnknownMemberName_NullIsReturned()
    {
      //Arrange
      var memberExpression = GetTitleMemberExpression();
      entityMetadata.Add(memberExpression);

      //Act
      var result = entityMetadata.TryGetMemberExpression(nameof(Foo.Title));

      //Assert
      result.Should().NotBeNull();
      result!.Member.GetCustomAttribute<JsonPropertyNameAttribute>().Should().NotBeNull();
    }

    [Test]
    public void TryGetMemberExpression_KnownMemberName_MemberExpressionIsReturned()
    {
      //Arrange
      var memberExpression = GetTitleMemberExpression();
      entityMetadata.Add(memberExpression);

      //Act
      var result = entityMetadata.TryGetMemberExpression(nameof(Foo.Id));

      //Assert
      result.Should().BeNull();
    }

    private record Foo
    {
      public string Id { get; set; } = null!;

      [JsonPropertyName("title")]
      public string Title { get; init; } = null!;
    }
  }

}
