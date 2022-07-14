using System;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

[TestClass]
public class OperatorBetweenKSqlVisitorTests : TestBase
{
  [TestMethod]
  public void Visit_Between()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = t => t.Id.Between(1, 100);
    StringBuilder stringBuilder = new();

    //Act
    new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
    var ksql = stringBuilder.ToString();

    //Assert
    ksql.Should().Be("Id BETWEEN 1 AND 100");
  }

  [TestMethod]
  public void Visit_NotBetween()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = t => t.Id.NotBetween(1, 100);
    StringBuilder stringBuilder = new();

    //Act
    new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
    var ksql = stringBuilder.ToString();

    //Assert
    ksql.Should().Be("Id NOT BETWEEN 1 AND 100");
  }

  private struct MyTimeSpan
  {
    public TimeSpan Ts { get; set; }
    public DateTime Dt { get; set; }
    public DateTimeOffset DtOffset { get; set; }
  }

  [TestMethod]
  public void BetweenTime()
  {
    //Arrange
    var from = new TimeSpan(11, 0, 0);
    var to = new TimeSpan(15,0 , 0);

    Expression<Func<MyTimeSpan, bool>> expression = t => t.Ts.Between(from, to);
    StringBuilder stringBuilder = new();

    //Act
    new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
    var ksql = stringBuilder.ToString();

    //Assert
    ksql.Should().Be("Ts BETWEEN '11:00:00' AND '15:00:00'");
  }

  [TestMethod]
  public void NotBetweenTime()
  {
    //Arrange
    var from = new TimeSpan(11, 0, 0);
    var to = new TimeSpan(15,0 , 0);

    Expression<Func<MyTimeSpan, bool>> expression = t => t.Ts.NotBetween(from, to);
    StringBuilder stringBuilder = new();

    //Act
    new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
    var ksql = stringBuilder.ToString();

    //Assert
    ksql.Should().Be("Ts NOT BETWEEN '11:00:00' AND '15:00:00'");
  }

  [TestMethod]
  public void BetweenNewTime()
  {
    //Arrange
    Expression<Func<MyTimeSpan, bool>> expression = t => t.Ts.Between(new TimeSpan(11, 0, 0), new TimeSpan(15, 0, 0));
    StringBuilder stringBuilder = new();

    //Act
    new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
    var ksql = stringBuilder.ToString();

    //Assert
    ksql.Should().Be("Ts BETWEEN '11:00:00' AND '15:00:00'");
  }

  [TestMethod]
  public void BetweenDate()
  {
    //Arrange
    var from = new DateTime(2021, 10, 1);
    var to = new DateTime(2021, 10, 12);

    Expression<Func<MyTimeSpan, bool>> expression = t => t.Dt.Between(from, to);
    StringBuilder stringBuilder = new();

    //Act
    new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
    var ksql = stringBuilder.ToString();

    //Assert
    ksql.Should().Be($"{nameof(MyTimeSpan.Dt)} BETWEEN '2021-10-01' AND '2021-10-12'");
  }

  [TestMethod]
  public void BetweenDateTimeOffset()
  {
    //Arrange
    var from = new DateTimeOffset(new DateTime(2021, 10, 1), TimeSpan.Zero);
    var to = new DateTimeOffset(new DateTime(2021, 10, 12), TimeSpan.Zero);

    Expression<Func<MyTimeSpan, bool>> expression = t => t.DtOffset.Between(from, to);
    StringBuilder stringBuilder = new();

    //Act
    new OperatorBetweenKSqlVisitor(stringBuilder, new KSqlQueryMetadata()).Visit(expression);
    var ksql = stringBuilder.ToString();

    //Assert
    ksql.Should().Be($"{nameof(MyTimeSpan.DtOffset)} BETWEEN '2021-10-01T00:00:00.000+00:00' AND '2021-10-12T00:00:00.000+00:00'");
  }
}