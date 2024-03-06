using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors;

public class KSqlFunctionVisitorTests : TestBase
{
  private KSqlFunctionVisitor ClassUnderTest { get; set; } = null!;

  private StringBuilder StringBuilder { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlFunctionVisitor(StringBuilder, new KSqlQueryMetadata());
  }

  #region Nulls
    
  [Test]
  public void IfNull_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = c => K.Functions.IfNull(c.Message, "x");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"IFNULL({nameof(Tweet.Message)}, 'x')");
  }

  private readonly struct TweetMessage
  {
    public User User { get; init; }
  }

  private readonly struct User
  {
    public string Description { get; init; }
  }

  [Test]
  public void IfNullStruct_BuildKSql_PrintsFunction()
  {
    //Arrange
    string altValue = "x";
    Expression<Func<TweetMessage, string>> expression = c => K.Functions.IfNull(c.User.Description, altValue);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"IFNULL({nameof(User)}->{nameof(User.Description)}, '{altValue}')");
  }

  #endregion

  #region Dynamic

  [Test]
  public void Dynamic_BuildKSql_PrintsStringifiedFunction()
  {
    //Arrange
    string functionCall = "IFNULL(Message, 'n/a')";
    Expression<Func<Tweet, object>> expression = c => K.Functions.Dynamic(functionCall);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{functionCall}");
  }

  [Test]
  public void DynamicNewExpression_BuildKSql_PrintsStringifiedFunction()
  {
    //Arrange
    string functionCall = "IFNULL(Message, 'n/a')";
    Expression<Func<Tweet, object>> expression = c => new { Col = K.Functions.Dynamic(functionCall) as string};

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{functionCall} Col");
  }

  [Test]
  public void DynamicNewExpressionMultipleFields_BuildKSql_PrintsStringifiedFunction()
  {
    //Arrange
    string functionCall = "IFNULL(Message, 'n/a')";
    Expression<Func<Tweet, object>> expression = c => new { c.Id, c.Amount, Col = K.Functions.Dynamic(functionCall) as string};

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Tweet.Id)}, {nameof(Tweet.Amount)}, {functionCall} Col");
  }

  #endregion

  #region Arrays
    
  [Test]
  public void Array_BuildKSql_PrintsArray()
  {
    //Arrange
    Expression<Func<int[]>> expression = () => new[] { 1, 2, 3 };
      
    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("ARRAY[1, 2, 3]");
  }
    
  [Test]
  public void ArrayDestructure_BuildKSql_PrintsIndexer()
  {
    //Arrange
    Expression<Func<int>> expression = () => new[] { 1, 2, 3 }[1];

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("ARRAY[1, 2, 3][1]");
  }
    
  [Test]
  public void ArrayLength_BuildKSql_PrintsArrayLength()
  {
    //Arrange
    Expression<Func<int>> expression = () => new[] { 1, 2, 3 }.Length;

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("ARRAY_LENGTH(ARRAY[1, 2, 3])");
  }

  #endregion
}
