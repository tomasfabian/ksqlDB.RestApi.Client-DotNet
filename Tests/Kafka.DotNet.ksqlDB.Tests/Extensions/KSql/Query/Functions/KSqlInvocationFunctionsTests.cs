using System;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Kafka.DotNet.ksqlDB.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Functions
{
  [TestClass]
  public class KSqlInvocationFunctionsTests : TestBase
  {
    private KSqlInvocationFunctionVisitor ClassUnderTest { get; set; }

    private StringBuilder StringBuilder { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      StringBuilder = new StringBuilder();
      ClassUnderTest = new KSqlInvocationFunctionVisitor(StringBuilder);
    }

    class Tweets
    {
      public int Id { get; set; }
      public string[] Messages { get; set; }
      public int[] Values { get; set; }
    }

    [TestMethod]
    public void Transform()
    {
      //Arrange
      Expression<Func<Tweets, string[]>> expression = c => K.Functions.Transform(c.Messages, x => x.ToUpper());
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be($"TRANSFORM({nameof(Tweets.Messages)}, (x) => UCASE(x))");
    }

    [TestMethod]
    public void Transform_Constant()
    {
      //Arrange
      Expression<Func<Tweets, int[]>> expression = c => K.Functions.Transform(c.Values, x => x + 1);
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be($"TRANSFORM({nameof(Tweets.Values)}, (x) => x + 1)");
    }

    [TestMethod]
    public void Transform_Function()
    {
      //Arrange
      Expression<Func<Tweets, string[]>> expression = c => K.Functions.Transform(c.Messages, y => K.Functions.Concat(y, "_new"));
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be($"TRANSFORM({nameof(Tweets.Messages)}, (y) => CONCAT(y, '_new'))");
    }
    
    [TestMethod]
    public void Transform_AnonymousType()
    {
      //Arrange
      Expression<Func<Tweets, object>> expression = c => new { Col = K.Functions.Transform(c.Values, x => x + 1)};
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be($"TRANSFORM({nameof(Tweets.Values)}, (x) => x + 1) Col");
    }

    [TestMethod]
    public void Filter()
    {
      //Arrange
      Expression<Func<Tweets, string[]>> expression = c => K.Functions.Filter(c.Messages, x => x == "E.T.");
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be($"FILTER({nameof(Tweets.Messages)}, (x) => x = 'E.T.')");
    }

    [TestMethod]
    public void Reduce()
    {
      //Arrange
      Expression<Func<Tweets, int>> expression = c => K.Functions.Reduce(c.Values, 0, (x,y) => x + y);
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be($"REDUCE({nameof(Tweets.Values)}, 0, (x, y) => x + y)");
    }
  }
}