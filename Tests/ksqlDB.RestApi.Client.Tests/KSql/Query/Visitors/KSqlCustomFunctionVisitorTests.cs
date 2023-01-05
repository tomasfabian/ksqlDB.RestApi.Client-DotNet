using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

[TestClass]
public class KSqlCustomFunctionVisitorTests : TestBase
{
  private KSqlCustomFunctionVisitor ClassUnderTest { get; set; } = null!;

  private StringBuilder StringBuilder { get; set; } = null!;

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlCustomFunctionVisitor(StringBuilder, new KSqlQueryMetadata());

    KSqlDBContextOptions.NumberFormatInfo = new System.Globalization.NumberFormatInfo
    {
      NumberDecimalSeparator = "."
    };
  }

  [KSqlFunction]
  string Substring(string input, int position, int length) => throw new NotSupportedException();

  [TestMethod]
  public void InstanceFunction()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = c => Substring(c.Message, 2, 3);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Substring({nameof(Tweet.Message)}, 2, 3)");
  }

  [TestMethod]
  public void MissingAttribute_NothingIsRendered()
  {
    //Arrange
    Expression<Func<Tweet, string?>> expression = c => c.ToString();

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(String.Empty);
  }

  private static class F
  {
    [KSqlFunction]
    public static double Abs(double input) => throw new NotSupportedException();

    [KSqlFunction(FunctionName = "Abs")]
    public static double MyAbs(double input) => throw new NotSupportedException();
  }

  [TestMethod]
  public void StaticFunction()
  {
    Expression<Func<Tweet, double>> expression = c => F.Abs(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ABS({nameof(Tweet.Amount)})");
  }

  [TestMethod]
  public void StaticFunction_OverridenFunctionName()
  {
    Expression<Func<Tweet, double>> expression = c => F.Abs(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"Abs({nameof(Tweet.Amount)})");
  }
}
