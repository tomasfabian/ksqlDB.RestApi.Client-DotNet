using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.KSql.RestApi.Generators;
using ksqlDb.RestApi.Client.Tests.Models.Movies;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors
{
  public class ConstantVisitorTests
  {
    private StringBuilder stringBuilder = null!;
    private KSqlQueryMetadata queryMetadata = null!;
    private ConstantVisitor constantVisitor = null!;

    [SetUp]
    public void Setup()
    {
      stringBuilder = new StringBuilder();
      queryMetadata = new KSqlQueryMetadata();
      constantVisitor = new ConstantVisitor(stringBuilder, queryMetadata);
    }

    [Test]
    public void PrimitiveConstant()
    {
      //Arrange
      Expression expression = Expression.Constant(1);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().Be("1");
    }

    [Test]
    public void ListSortDirectionAscendingConstant()
    {
      //Arrange
      Expression expression = Expression.Constant(ListSortDirection.Ascending);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().Be("'ASC'");
    }

    [Test]
    public void ListSortDirectionDescendingConstant()
    {
      //Arrange
      Expression expression = Expression.Constant(ListSortDirection.Descending);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().Be("'DESC'");
    }

    [Test]
    public void StringConstant()
    {
      //Arrange
      string value = "hello";
      Expression expression = Expression.Constant(value);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().Be($"'{value}'");
    }

    [Test]
    public void DoubleConstant()
    {
      //Arrange
      KSqlDBContextOptions.NumberFormatInfo = new System.Globalization.NumberFormatInfo
      {
        NumberDecimalSeparator = ","
      };

      double value = 3.1;
      Expression expression = Expression.Constant(value);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().Be("3,1");
    }

    [Test]
    public void EnumerableConstant()
    {
      //Arrange
      var values = new List<int> {1, 2};
      Expression expression = Expression.Constant(values);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().BeEquivalentTo("ARRAY[1, 2]");
    }

    [Test]
    public void IsInContains_EnumerableConstant()
    {
      //Arrange
      queryMetadata = new KSqlQueryMetadata
      {
        IsInContainsScope = true
      };
      constantVisitor = new ConstantVisitor(stringBuilder, queryMetadata);

      var values = new List<int> {1, 2};
      Expression expression = Expression.Constant(values);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().BeEquivalentTo("1, 2");
    }

    [Test]
    public void ClassInstanceConstant()
    {
      //Arrange
      var value = new Movie
      {
        Title = "title"
      };
      Expression expression = Expression.Constant(value);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().BeEquivalentTo("STRUCT(Title := 'title', Id := 0, Release_Year := 0)");
    }

    [Test]
    public void Enum()
    {
      //Arrange
      var value = new Port
      {
        Id = 42,
        PortType = PortType.Kafka
      };
      Expression expression = Expression.Constant(value);

      //Act
      constantVisitor.Visit(expression);

      //Assert
      var ksql = stringBuilder.ToString();
      ksql.Should().BeEquivalentTo("STRUCT(Id := 42, PortType := 'Kafka')");
    }
  }
}
