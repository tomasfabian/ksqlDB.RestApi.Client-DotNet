using System;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors
{
  [TestClass]
  public class KSqlFunctionVisitorTests : TestBase
  {
    private KSqlFunctionVisitor ClassUnderTest { get; set; }

    private StringBuilder StringBuilder { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      StringBuilder = new StringBuilder();
      ClassUnderTest = new KSqlFunctionVisitor(StringBuilder, useTableAlias: false);
    }

    #region Nulls
    
    [TestMethod]
    public void IfNull_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = c => K.Functions.IfNull(c.Message, "x");

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"IFNULL({nameof(Tweet.Message)}, 'x')");
    }

    private struct TweetMessage
    {
      public User User { get; init; }
    }

    private struct User
    {
      public string Description { get; init; }
    }

    [TestMethod]
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

    [TestMethod]
    public void Dynamic_BuildKSql_PrintsStringifiedFunction()
    {
      //Arrange
      string functionCall = "IFNULL(Message, 'n/a')";
      Expression<Func<Tweet, object>> expression = c => K.Functions.Dynamic(functionCall) as string;

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"{functionCall}");
    }

    [TestMethod]
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

    [TestMethod]
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
    
    [TestMethod]
    public void Array_BuildKSql_PrintsArray()
    {
      //Arrange
      Expression<Func<int[]>> expression = () => new[] { 1, 2, 3 };
      
      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("ARRAY[1, 2, 3]");
    }
    
    [TestMethod]
    public void ArrayDestructure_BuildKSql_PrintsIndexer()
    {
      //Arrange
      Expression<Func<int>> expression = () => new[] { 1, 2, 3 }[1];

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("ARRAY[1, 2, 3][1]");
    }
    
    [TestMethod]
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
}