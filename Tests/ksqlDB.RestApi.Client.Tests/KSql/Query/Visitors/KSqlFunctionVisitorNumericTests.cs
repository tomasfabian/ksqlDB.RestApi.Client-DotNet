using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors;

public class KSqlFunctionVisitorNumericTests : TestBase
{
  private KSqlFunctionVisitor ClassUnderTest { get; set; } = null!;

  private StringBuilder StringBuilder { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlFunctionVisitor(StringBuilder, new KSqlQueryMetadata());
      
    KSqlDBContextOptions.NumberFormatInfo = new System.Globalization.NumberFormatInfo
    {
      NumberDecimalSeparator = "."
    };
  }

  #region Abs

  [Test]
  public void DoubleAbs_BuildKSql_PrintsAbsFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Abs(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ABS({nameof(Tweet.Amount)})");
  }

  [Test]
  public void DecimalAbs_BuildKSql_PrintsAbsFunction()
  {
    //Arrange
    Expression<Func<Tweet, decimal>> expression = c => K.Functions.Abs(c.AccountBalance);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ABS({nameof(Tweet.AccountBalance)})");
  }

  #endregion

  #region Ceil

  [Test]
  public void DoubleCeil_BuildKSql_PrintsCeilFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Ceil(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"CEIL({nameof(Tweet.Amount)})");
  }

  [Test]
  public void DecimalCeil_BuildKSql_PrintsCeilFunction()
  {
    //Arrange
    Expression<Func<Tweet, decimal>> expression = c => K.Functions.Ceil(c.AccountBalance);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"CEIL({nameof(Tweet.AccountBalance)})");
  }

  #endregion

  #region Entries

  private class Test
  {
    public IDictionary<string, string> Dictionary { get; set; } = null!;
  }

  [Test]
  public void Entries_BuildKSql_PrintsFunction()
  {
    //Arrange
    bool sorted = true;
    Expression<Func<Test, Entry<string>[]>> expression = c => KSqlFunctions.Instance.Entries(c.Dictionary, sorted);

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo($"ENTRIES({nameof(Test.Dictionary)}, true)");
  }

  [Test]
  public void EntriesFromDictionary_BuildKSql_PrintsFunction()
  {
    //Arrange
    bool sorted = true;
    Expression<Func<Test, Entry<string>[]>> expression = c => KSqlFunctions.Instance.Entries(new Dictionary<string, string>()
    {
      { "a", "value" }
    }, sorted);

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo("ENTRIES(MAP('a' := 'value'), true)");
  }

  [Test]
  public void EntriesOuterMemberAccess_BuildKSql_PrintsFunction()
  {
    //Arrange
    var map = new Dictionary<string, string>()
    {
      { "a", "value" }
    };

    bool sorted = true;

    Expression<Func<Test, Entry<string>[]>> expression = c => KSqlFunctions.Instance.Entries(map, sorted);

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().Be("ENTRIES(MAP('a' := 'value'), True)");
  }

  #endregion

  #region Exp

  [Test]
  public void Exp_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Exp(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"EXP({nameof(Tweet.Amount)})");
  }

  #endregion

  #region Floor

  [Test]
  public void DoubleFloor_BuildKSql_PrintsFloorFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Floor(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"FLOOR({nameof(Tweet.Amount)})");
  }

  [Test]
  public void DecimalFloor_BuildKSql_PrintsFloorFunction()
  {
    //Arrange
    Expression<Func<Tweet, decimal>> expression = c => K.Functions.Floor(c.AccountBalance);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"FLOOR({nameof(Tweet.AccountBalance)})");
  }

  #endregion

  #region GenerateSeries

  [Test]
  public void GenerateSeries_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Transaction, long[]>> expression = c => K.Functions.GenerateSeries(c.RowTime, 1, 5);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"GENERATE_SERIES({nameof(Transaction.RowTime)}, 1, 5)");
  }

  #endregion

  #region GeoDistance

  [Test]
  public void GeoDistance_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Location, double>> expression = c => K.Functions.GeoDistance(c.Longitude, 1.1, 2, 3);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"GEO_DISTANCE({nameof(Location.Longitude)}, 1.1, 2, 3)");
  }

  [Test]
  public void GeoDistanceWithUnit_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Location, double>> expression = c => K.Functions.GeoDistance(c.Longitude, 1.1, 2, 3, "MI");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"GEO_DISTANCE({nameof(Location.Longitude)}, 1.1, 2, 3, 'MI')");
  }

  #endregion

  #region Ln

  [Test]
  public void Ln_BuildKSql_PrintsFloorFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Ln(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"LN({nameof(Tweet.Amount)})");
  }

  #endregion

  #region Round

  [Test]
  public void DoubleRound_BuildKSql_PrintsRoundFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Round(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ROUND({nameof(Tweet.Amount)})");
  }

  [Test]
  public void DoubleRoundWithScale_BuildKSql_PrintsRoundFunction()
  {
    //Arrange
    int scale = 3;
    Expression<Func<Tweet, double>> expression = c => K.Functions.Round(c.Amount, scale);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ROUND({nameof(Tweet.Amount)}, {scale})");
  }

  [Test]
  public void DecimalRound_BuildKSql_PrintsRoundFunction()
  {
    //Arrange
    Expression<Func<Tweet, decimal>> expression = c => K.Functions.Round(c.AccountBalance);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ROUND({nameof(Tweet.AccountBalance)})");
  }

  #endregion

  #region Random
    
  [Test]
  public void Random_BuildKSql_PrintsRandomFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Random();

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("RANDOM()");
  }

  #endregion

  #region Sign

  [Test]
  public void DoubleSign_BuildKSql_PrintsSignFunction()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => K.Functions.Sign(c.Amount);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"SIGN({nameof(Tweet.Amount)})");
  }

  #endregion
}
