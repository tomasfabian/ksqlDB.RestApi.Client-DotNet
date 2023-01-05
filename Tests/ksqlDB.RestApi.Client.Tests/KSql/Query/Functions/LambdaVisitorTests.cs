using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Functions;

[TestClass]
public class LambdaVisitorTests : TestBase
{
  private LambdaVisitor ClassUnderTest { get; set; } = null!;

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();
      
    ClassUnderTest = new LambdaVisitor(new StringBuilder(), new KSqlQueryMetadata());
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

  [TestMethod]
  public void DateType()
  {
    //Arrange
    Expression<Func<IDictionary<string, int>, object>> expression = c => new DateTime(2021, 3, 7);

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be("(c) => '2021-03-07'");
  }
}
