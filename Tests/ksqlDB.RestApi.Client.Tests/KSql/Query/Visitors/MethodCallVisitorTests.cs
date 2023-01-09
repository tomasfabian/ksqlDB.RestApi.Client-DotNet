using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using System.Text;
using ksqlDB.Api.Client.Tests.Models;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

public class MethodCallVisitorTests
{
  [Test]
  public void GetMapValue()
  {
    //Arrange
    Expression<Func<Transaction, int>> expression = t => t.Dictionary["value"];
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("Dictionary['value']");
  }

  [Test]
  public void CallAFunction()
  {
    //Arrange
    Expression<Func<Transaction, int>> expression = t => K.Functions.Instr(t.CardNumber, "123");
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("INSTR(CardNumber, '123')");
  }

  [Test]
  public void CallAnAggregationFunction()
  {
    //Arrange
    Expression<Func<IAggregations, int>> expression = t => t.Count();
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("COUNT(*)");
  }

  [Test]
  public void ColumnValue_Between()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = t => t.Amount.Between(1, 2);
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("Amount BETWEEN 1 AND 2");
  }

  [Test]
  public void Between()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = t => 1.Between(1, 3);
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("1 BETWEEN 1 AND 3");
  }

  [Test]
  public void StringToUpper()
  {
    //Arrange
    Expression<Func<Transaction, string>> expression = t => t.CardNumber.ToUpper();
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("UCASE(CardNumber)");
  }

  [Test]
  public void InvocationFunction()
  {
    //Arrange
    Expression<Func<Transaction, int>> expression = t => t.Array.Reduce(0, (x, y) =>
      x + y
    );
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("REDUCE(Array, 0, (x, y) => x + y)");
  }

  [Test]
  public void CastAsVarchar()
  {
    //Arrange
    Expression<Func<string>> expression = () => 1.ToString();
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("CAST(1 AS VARCHAR)");
  }

  [Test]
  public void CastAsInt()
  {
    //Arrange
    Expression<Func<int>> expression = () => Convert.ToInt32("22");
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("CAST('22' AS INT)");
  }

  [Test]
  public void CastAsBigInt()
  {
    //Arrange
    Expression<Func<long>> expression = () => Convert.ToInt64("22");
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("CAST('22' AS BIGINT)");
  }

  [Test]
  public void CastAsDouble()
  {
    //Arrange
    Expression<Func<double>> expression = () => Convert.ToDouble("22");
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("CAST('22' AS DOUBLE)");
  }

  [Test]
  public void CastAsDecimal()
  {
    //Arrange
    Expression<Func<decimal>> expression = () => KSQLConvert.ToDecimal("22", 10, 2);
    StringBuilder stringBuilder = new();
    MethodCallVisitor visitor = new(stringBuilder, new KSqlQueryMetadata());

    //Act
    visitor.Visit(expression);

    //Assert
    var ksql = stringBuilder.ToString();
    ksql.Should().Be("CAST('22' AS DECIMAL(10,2))");
  }
}
