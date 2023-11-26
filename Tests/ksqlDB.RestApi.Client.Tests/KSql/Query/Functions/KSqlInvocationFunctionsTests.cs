using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.KSql.Linq;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Functions;

public class KSqlInvocationFunctionsTests : TestBase
{
  private KSqlInvocationFunctionVisitor ClassUnderTest { get; set; } = null!;

  private StringBuilder StringBuilder { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlInvocationFunctionVisitor(StringBuilder, new KSqlQueryMetadata());
  }

  class Tweets
  {
    public int Id { get; set; }
    public string[] Messages { get; init; } = null!;
    public int[] Values { get; init; } = null!;
    public IDictionary<string, int[]> Dictionary { get; init; } = null!;
    public IDictionary<string, int> Dictionary2 { get; init; } = null!;
    public IDictionary<string, QbservableGroupByExtensionsTests.City> Dictionary3 { get; init; } = null!;
  }

  #region Array

  [Test]
  public void Transform()
  {
    //Arrange
    Expression<Func<Tweets, string[]>> expression = c => K.Functions.Transform(c.Messages, x => x.ToUpper());
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Messages)}, (x) => UCASE(x))");
  }

  [Test]
  public void TransformExtensionMethod()
  {
    //Arrange
    Expression<Func<Tweets, string[]>> expression = c => c.Messages.Transform(x => x.ToUpper());

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Messages)}, (x) => UCASE(x))");
  }

  [Test]
  public void TransformExtensionMethod_Dictionary()
  {
    //Arrange
    Expression<Func<Tweets, IDictionary<string, string>>> expression = c => c.Dictionary3.Transform((k,v) => k, (k, v) => v.RegionCode);

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Dictionary3)}, (k, v) => k, (k, v) => v->RegionCode)");
  }

  [Test]
  [Ignore("TODO:")]
  public void TransformExtensionMethod_Dictionary_ToUpper()
  {
    //Arrange
    Expression<Func<Tweets, IDictionary<string, string>>> expression = c => c.Dictionary3.Transform((k, v) => k, (k, v) => v.RegionCode.ToUpper());

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Dictionary3)}, (k, v) => k, (k, v) => UCASE(v->RegionCode))");
  }

  [Test]
  public void Transform_Constant()
  {
    //Arrange
    Expression<Func<Tweets, int[]>> expression = c => K.Functions.Transform(c.Values, x => x + 1);
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Values)}, (x) => x + 1)");
  }

  [Test]
  public void Transform_Function()
  {
    //Arrange
    Expression<Func<Tweets, string[]>> expression = c => K.Functions.Transform(c.Messages, y => K.Functions.Concat(y, "_new"));
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Messages)}, (y) => CONCAT(y, '_new'))");
  }

  [Test]
  public void Transform_Function_NestedPropertyAccessor()
  {
    //Arrange
    Expression<Func<Tweets, IDictionary<string ,string>>> expression = c => K.Functions.Transform(c.Dictionary3, (k, v) => k, (k, v) => v.State.Name);
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Dictionary3)}, (k, v) => k, (k, v) => v->State->Name)");
  }

  [Test]
  public void Transform_Function_NestedPropertyAccessorAndLength()
  {
    //Arrange
    Expression<Func<Tweets, IDictionary<string, int>>> expression = c => K.Functions.Transform(c.Dictionary3, (k, v) => k, (k, v) => v.State.Name.Length);
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Dictionary3)}, (k, v) => k, (k, v) => LEN(v->State->Name))");
  }

  [Test]
  public void Transform_AnonymousType()
  {
    //Arrange
    Expression<Func<Tweets, object>> expression = c => new { Col = K.Functions.Transform(c.Values, x => x + 1)};
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Values)}, (x) => x + 1) Col");
  }
    
  [Test]
  public void Transform_CapturedVariable_AnonymousType()
  {
    //Arrange
    int value = 1;
    Expression<Func<Tweets, object>> expression = c => new { Col = K.Functions.Transform(c.Values, x => x + value)};
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Values)}, (x) => x + 1) Col");
  }

  [Test]
  public void Filter()
  {
    //Arrange
    Expression<Func<Tweets, string[]>> expression = c => K.Functions.Filter(c.Messages, x => x == "E.T.");
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"FILTER({nameof(Tweets.Messages)}, (x) => x = 'E.T.')");
  }

  [Test]
  public void FilterExtensionMethod()
  {
    //Arrange
    Expression<Func<Tweets, string[]>> expression = c => c.Messages.Filter(x => x == "E.T.");

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"FILTER({nameof(Tweets.Messages)}, (x) => x = 'E.T.')");
  }

  [Test]
  public void Reduce()
  {
    //Arrange
    Expression<Func<Tweets, int>> expression = c => K.Functions.Reduce(c.Values, 0, (x,y) => x + y);
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"REDUCE({nameof(Tweets.Values)}, 0, (x, y) => x + y)");
  }

  [Test]
  public void ReduceExtensionMethod()
  {
    //Arrange
    Expression<Func<Tweets, int>> expression = c => c.Values.Reduce(0, (x,y) => x + y);
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"REDUCE({nameof(Tweets.Values)}, 0, (x, y) => x + y)");
  }

  #endregion

  #region Map

  [Test]
  public void TransformMap()
  {
    //Arrange
    Expression<Func<Tweets, IDictionary<string, int[]>>> expression = c => K.Functions.Transform(c.Dictionary, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v, x => x * x));
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Dictionary)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v, (x) => x * x))");
  }

  [Test]
  public void TransformMap_NestedTransformWithPropertyAccessor()
  {
    //Arrange
    Expression<Func<Tweets, IDictionary<string, int[]>>> expression = c => K.Functions.Transform(c.Dictionary3, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v.Values, x => x * x));
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"TRANSFORM({nameof(Tweets.Dictionary3)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->Values, (x) => x * x))");
  }

  [Test]
  public void TransformMapTwice()
  {
    //Arrange
    Expression<Func<Tweets, object>> expression = 
      c => new { A = K.Functions.Transform(c.Dictionary3, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v.Values, x => x * x)), B = K.Functions.Transform(c.Dictionary3, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v.Values, x => x * x))};
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    string expected =
      $"TRANSFORM({nameof(Tweets.Dictionary3)}, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v->Values, (x) => x * x))";

    ksql.Should().Be($"{expected} A, {expected} B");
  }

  [Test]
  public void FilterMap()
  {
    //Arrange
    Expression<Func<Tweets, IDictionary<string, int>>> expression = c => K.Functions.Filter(c.Dictionary2, (k, v) => K.Functions.Instr(k, "name") > 0 && k != "E.T" && v > 0);
      
    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"FILTER({nameof(Tweets.Dictionary2)}, (k, v) => ((INSTR(k, 'name') > 0) AND (k != 'E.T')) AND (v > 0))");
  }

  [Test]
  public void ReduceMap()
  {
    //Arrange
    Expression<Func<Tweets, int>> expression = c => K.Functions.Reduce(c.Dictionary2, 2, (s, k, v) => K.Functions.Ceil(s / v));

    //Act
    var ksql = ClassUnderTest.BuildKSql(expression);

    //Assert
    ksql.Should().Be($"REDUCE({nameof(Tweets.Dictionary2)}, 2, (s, k, v) => CEIL(s / v))");
  }

  #endregion
}
