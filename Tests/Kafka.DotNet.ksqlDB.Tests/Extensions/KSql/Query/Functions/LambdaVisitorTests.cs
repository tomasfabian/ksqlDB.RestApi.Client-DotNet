using System;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Functions
{
  [TestClass]
  public class LambdaVisitorTests : TestBase
  {
    private LambdaVisitor ClassUnderTest { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();
      
      ClassUnderTest = new LambdaVisitor(new StringBuilder());
    }
    
    [TestMethod]
    public void SingleLambdaParam()
    {
      //Arrange
      Expression<Func<string, string>> expression = x => x.ToUpper();
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(x) => UCASE(x)");
    }
    
    [TestMethod]
    public void MultipleLambdaParams()
    {
      //Arrange
      Expression<Func<int, int, int>> expression = (x, y) => x + y;
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(x, y) => x + y");
    }
  }
}