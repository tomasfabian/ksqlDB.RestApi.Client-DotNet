using ksqlDB.RestApi.Client.KSql.Query.Functions;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

using Models;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Linq.Expressions;
using System.Text;

public class LambdaVisitorTests
{
  private StringBuilder stringBuilder = null!;
  private KSqlQueryMetadata queryMetadata = null!;
  private LambdaVisitor lambdaVisitor = null!;

  [SetUp]
  public void Setup()
  {
    stringBuilder = new StringBuilder();
    queryMetadata = new KSqlQueryMetadata();
    lambdaVisitor = new LambdaVisitor(stringBuilder, queryMetadata);
  }

  [Test]
  public void Visit_Length_ShouldBeAppendedCorrectly()
  {
    //Arrange
    Expression<Func<Tweet, bool>> expression = c => c.Message.Length > 0;

    //Act
    lambdaVisitor.Visit(expression);

    //Assert
    var result = stringBuilder.ToString();
    result.Should().BeEquivalentTo($"(c) => LEN(c->{nameof(Tweet.Message)}) > 0");
  }

  [Test]
  public void Visit_MultipleArguments_ShouldBeAppendedCorrectly()
  {
    //Arrange
    Expression<Func<Dictionary<string, int>, int>> expression = c => c.Values.Reduce(0, (x,y) => x + y);

    //Act
    lambdaVisitor.Visit(expression);

    //Assert
    var result = stringBuilder.ToString();
    result.Should().BeEquivalentTo("(c) => REDUCE(c->Values, 0, (x, y) => x + y)");
  }
}
