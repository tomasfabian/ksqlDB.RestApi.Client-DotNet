using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors;

public class StringVisitorTests
{
  private StringBuilder stringBuilder = null!;
  private KSqlQueryMetadata queryMetadata = null!;
  private StringVisitor stringVisitor = null!;

  [SetUp]
  public void Setup()
  {
    stringBuilder = new StringBuilder();
    queryMetadata = new KSqlQueryMetadata();
    stringVisitor = new StringVisitor(stringBuilder, queryMetadata);
  }

  [Test]
  public void Visit_ToUpper_AppendsCorrectly()
  {
    //Arrange
    Expression<Func<string>> expression = () => "text".ToUpper();

    //Act
    _ = stringVisitor.Visit(expression);
    var result = stringBuilder.ToString();

    //Assert
    result.Should().BeEquivalentTo("UCASE('text')");
  }

  [Test]
  public void Visit_ToLower_AppendsCorrectly()
  {
    //Arrange
    Expression<Func<string>> expression = () => "text".ToLower();

    //Act
    _ = stringVisitor.Visit(expression);
    var result = stringBuilder.ToString();

    //Assert
    result.Should().BeEquivalentTo("LCASE('text')");
  }

  [Test]
  public void Visit_StartsWith_AppendsLike()
  {
    //Arrange
    string text = "te";
    Expression<Func<bool>> expression = () => "text".StartsWith(text);

    //Act
    _ = stringVisitor.Visit(expression);
    var result = stringBuilder.ToString();

    //Assert
    result.Should().BeEquivalentTo($"'text' LIKE '{text}%'");
  }

  [Test]
  public void Visit_EndsWith_AppendsLike()
  {
    //Arrange
    string text = "te";
    Expression<Func<bool>> expression = () => "text".EndsWith(text);

    //Act
    _ = stringVisitor.Visit(expression);
    var result = stringBuilder.ToString();

    //Assert
    result.Should().BeEquivalentTo($"'text' LIKE '%{text}'");
  }

  [Test]
  public void Visit_Contains_AppendsLike()
  {
    //Arrange
    string text = "te";
    Expression<Func<bool>> expression = () => "text".Contains(text);

    //Act
    _ = stringVisitor.Visit(expression);
    var result = stringBuilder.ToString();

    //Assert
    result.Should().BeEquivalentTo($"'text' LIKE '%{text}%'");
  }
}
