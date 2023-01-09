using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

public class BinaryVisitorTests
{
  [Test]
  public void ArrayIndex()
  {
    //Arrange
    int[] i = {3};
    Expression<Func<int>> expression = () => i[3];
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("ARRAY[3][3]");
  }

  [Test]
  public void Plus()
  {
    //Arrange
    int i = 2;
    Expression<Func<int>> expression = () => i + 3;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 + 3");
  }

  [Test]
  public void Minus()
  {
    //Arrange
    int i = 2;
    Expression<Func<int>> expression = () => i - 3;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 - 3");
  }

  [Test]
  public void Divide()
  {
    //Arrange
    int i = 2;
    Expression<Func<int>> expression = () => i / 3;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 / 3");
  }

  [Test]
  public void Multiply()
  {
    //Arrange
    int i = 2;
    Expression<Func<int>> expression = () => i * 3;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 * 3");
  }

  [Test]
  public void Modulo()
  {
    //Arrange
    int i = 2;
    Expression<Func<int>> expression = () => i % 3;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 % 3");
  }

  [Test]
  public void And()
  {
    //Arrange
    var i = true;
    Expression<Func<bool>> expression = () => i && true;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("True AND True");
  }

  [Test]
  public void Or()
  {
    //Arrange
    var i = false;
    Expression<Func<bool>> expression = () => i || true;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("False OR True");
  }

  [Test]
  public void Equal()
  {
    //Arrange
    int i = 2;
    Expression<Func<bool>> expression = () => i == 2;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 = 2");
  }

  [Test]
  public void Is()
  {
    //Arrange
    string? text = "test";
    Expression<Func<bool>> expression = () => text == null;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("'test' IS NULL");
  }

  [Test]
  public void IsNot()
  {
    //Arrange
    string? text = "test";
    Expression<Func<bool>> expression = () => text != null;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("'test' IS NOT NULL");
  }

  [Test]
  public void NotEqual()
  {
    //Arrange
    int i = 2;
    Expression<Func<bool>> expression = () => i != 3;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 != 3");
  }

  [Test]
  public void LessThan()
  {
    //Arrange
    int i = 2;
    Expression<Func<bool>> expression = () => i < 4;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 < 4");
  }

  [Test]
  public void LessThanOrEqual()
  {
    //Arrange
    int i = 2;
    Expression<Func<bool>> expression = () => i <= 4;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 <= 4");
  }

  [Test]
  public void GreaterThan()
  {
    //Arrange
    int i = 2;
    Expression<Func<bool>> expression = () => i > 4;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 > 4");
  }

  [Test]
  public void GreaterThanOrEqual()
  {
    //Arrange
    int i = 2;
    Expression<Func<bool>> expression = () => i >= 4;
    StringBuilder stringBuilder = new();
    BinaryVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("2 >= 4");
  }
}
