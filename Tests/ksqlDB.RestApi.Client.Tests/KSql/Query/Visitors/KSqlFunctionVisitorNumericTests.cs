using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors
{
  [TestClass]
  public class KSqlFunctionVisitorNumericTests : TestBase
  {
    private KSqlFunctionVisitor ClassUnderTest { get; set; }

    private StringBuilder StringBuilder { get; set; }

    [TestInitialize]
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

    [TestMethod]
    public void DoubleAbs_BuildKSql_PrintsAbsFunction()
    {
      //Arrange
      Expression<Func<Tweet, double>> expression = c => K.Functions.Abs(c.Amount);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ABS({nameof(Tweet.Amount)})");
    }

    [TestMethod]
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

    [TestMethod]
    public void DoubleCeil_BuildKSql_PrintsCeilFunction()
    {
      //Arrange
      Expression<Func<Tweet, double>> expression = c => K.Functions.Ceil(c.Amount);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"CEIL({nameof(Tweet.Amount)})");
    }

    [TestMethod]
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
      public IDictionary<string, string> Dictionary { get; set; }
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void DoubleFloor_BuildKSql_PrintsFloorFunction()
    {
      //Arrange
      Expression<Func<Tweet, double>> expression = c => K.Functions.Floor(c.Amount);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"FLOOR({nameof(Tweet.Amount)})");
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void GeoDistance_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Location, double>> expression = c => K.Functions.GeoDistance(c.Longitude, 1.1, 2, 3);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"GEO_DISTANCE({nameof(Location.Longitude)}, 1.1, 2, 3)");
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
    public void DoubleRound_BuildKSql_PrintsRoundFunction()
    {
      //Arrange
      Expression<Func<Tweet, double>> expression = c => K.Functions.Round(c.Amount);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ROUND({nameof(Tweet.Amount)})");
    }

    [TestMethod]
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

    [TestMethod]
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
    
    [TestMethod]
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

    [TestMethod]
    public void DoubleSign_BuildKSql_PrintsSignFunction()
    {
      //Arrange
      Expression<Func<Tweet, double>> expression = c => K.Functions.Sign(c.Amount);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"SIGN({nameof(Tweet.Amount)})");
    }

    [TestMethod]
    public void DecimalSign_BuildKSql_PrintsSignFunction()
    {
      //Arrange
      Expression<Func<Tweet, decimal>> expression = c => K.Functions.Sign(c.AccountBalance);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"SIGN({nameof(Tweet.AccountBalance)})");
    }

    #endregion
  }
}