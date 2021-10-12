using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Kafka.DotNet.ksqlDB.KSql.Query.Visitors;
using Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Linq;
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
    public void AnonymousTypeProjection()
    {
      //Arrange
      Expression<Func<string, object>> expression = x => new { Col = x.ToUpper() };
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(x) => UCASE(x) Col");
    }
    
    [TestMethod]
    public void K_Function()
    {
      //Arrange
      Expression<Func<string, object>> expression = c => K.Functions.Concat(c, "_new");
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(c) => CONCAT(c, '_new')");
    }

    [TestMethod]
    public void K_Function_MapPropertyAccessor()
    {
      //Arrange
      Expression<Func<IDictionary<string, int>, object>> expression = c => c["a"] + 1;
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(c) => c['a'] + 1");
    }
    
    class Container
    {
      public IDictionary<string, int> Dict { get; set; }
    }

    //[TestMethod]
    //public void K_Function_NestedMapPropertyAccessor()
    //{
    //  //Arrange
    //  Expression<Func<Container, object>> expression = c => c.Dict["a"] + 1;
      
    //  //Act
    //  var ksql = ClassUnderTest.BuildKSql(expression);

    //  //Assert
    //  ksql.Should().Be("(c) => c->Dict['a'] + 1");
    //}
    
    //[TestMethod]
    //public void K_Function_DeeplyNestedPropertyAccessor()
    //{
    //  //Arrange
    //  Expression<Func<QbservableGroupByExtensionsTests.City, object>> expression = c => K.Functions.Concat(c.State.Nested.Version, "_new");
      
    //  //Act
    //  var ksql = ClassUnderTest.BuildKSql(expression);

    //  //Assert
    //  ksql.Should().Be("(c) => CONCAT(c->State->Nested->Version, '_new')");
    //}
    
    //[TestMethod]
    //public void New_K_Function()
    //{
    //  //Arrange
    //  Expression<Func<string, object>> expression = c => new { C = K.Functions.Concat(c, "_new") };
      
    //  //Act
    //  var ksql = ClassUnderTest.BuildKSql(expression);

    //  //Assert
    //  ksql.Should().Be("(c) => CONCAT(c, '_new') C");
    //}
    
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
    
    [TestMethod]
    public void CapturedVariable()
    {
      //Arrange
      int i = 1;

      Expression<Func<int, int, int>> expression = (x, y) => x + i;
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(x, y) => x + 1");
    }
    
    [TestMethod]
    public void MultipleLambdaParams_Condition()
    {
      //Arrange
      Expression<Func<string, int, bool>> expression = (k, v) => v > 0;
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(k, v) => v > 0");
    }
    
    [TestMethod]
    public void MultipleLambdaParams_Conditions()
    {
      //Arrange
      Expression<Func<string, int, bool>> expression = (k, v) => k != "E.T" && v > 0;
      
      //Act
      var ksql = ClassUnderTest.BuildKSql(expression);

      //Assert
      ksql.Should().Be("(k, v) => (k != 'E.T') AND (v > 0)");
    }
  }
}