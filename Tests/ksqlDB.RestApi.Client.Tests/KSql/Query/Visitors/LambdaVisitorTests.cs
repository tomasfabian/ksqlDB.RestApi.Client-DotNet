using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
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

  [TestCase(IdentifierFormat.None, ExpectedResult = "(c) => LEN(c->MESSAGE) > 0")]
  [TestCase(IdentifierFormat.Keywords, ExpectedResult = "(c) => LEN(c->MESSAGE) > 0")]
  [TestCase(IdentifierFormat.Always, ExpectedResult = "(c) => LEN(c->`MESSAGE`) > 0")]
  public string Visit_Length_ShouldBeAppendedCorrectly(IdentifierFormat format)
  {
    //Arrange
    lambdaVisitor.QueryMetadata = new KSqlQueryMetadata { IdentifierFormat = format };
    Expression<Func<Tweet, bool>> expression = c => c.Message.Length > 0;

    //Act
    lambdaVisitor.Visit(expression);

    //Assert
    return stringBuilder.ToString();
  }

  [TestCase(IdentifierFormat.None, ExpectedResult = "(c) => REDUCE(c->Values, 0, (x, y) => x + y)")]
  [TestCase(IdentifierFormat.Keywords, ExpectedResult = "(c) => REDUCE(c->`Values`, 0, (x, y) => x + y)")]
  public string Visit_MultipleArguments_ShouldBeAppendedCorrectly(IdentifierFormat format)
  {
    //Arrange
    lambdaVisitor.QueryMetadata = new KSqlQueryMetadata { IdentifierFormat = format };
    Expression<Func<Dictionary<string, int>, int>> expression = c => c.Values.Reduce(0, (x,y) => x + y);

    //Act
    lambdaVisitor.Visit(expression);

    //Assert
    return stringBuilder.ToString();
  }
}
