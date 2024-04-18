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
      var memberInfo = GetTitleMemberInfo();

      //Act
      var added = entityMetadata.Add(memberInfo);

      //Assert
      added.Should().BeTrue();
    }

    private static MemberInfo GetTitleMemberInfo()
    {
      Expression<Func<Foo, object>> expression = foo => new { foo.Title };
      var argument = ((NewExpression)expression.Body).Arguments[0];
      var memberInfo = ((MemberExpression)argument).Member;
      return memberInfo;
    }

    [Test]
    public void Add_MemberWasNotStoredSecondTime()
    {
      //Arrange
      var memberInfo = GetTitleMemberInfo();
      entityMetadata.Add(memberInfo);

      //Act
      var added = entityMetadata.Add(GetTitleMemberInfo());

      //Assert
      added.Should().BeFalse();
    }

    [Test]
    public void TryGetMemberInfo_UnknownMemberName_NullIsReturned()
    {
      //Arrange
      var memberInfo = GetTitleMemberInfo();
      entityMetadata.Add(memberInfo);

      //Act
      var result = entityMetadata.TryGetMemberInfo(nameof(Foo.Title));

      //Assert
      result.Should().NotBeNull();
      result!.GetCustomAttribute<JsonPropertyNameAttribute>().Should().NotBeNull();
    }

    [Test]
    public void TryGetMemberInfo_KnownMemberName_MemberInfoIsReturned()
    {
      //Arrange
      var memberInfo = GetTitleMemberInfo();
      entityMetadata.Add(memberInfo);

      //Act
      var result = entityMetadata.TryGetMemberInfo(nameof(Foo.Id));

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
