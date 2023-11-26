using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors;

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
