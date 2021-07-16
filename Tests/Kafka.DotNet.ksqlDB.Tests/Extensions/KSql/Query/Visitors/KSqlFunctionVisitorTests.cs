using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Kafka.DotNet.ksqlDB.KSql.Query.Visitors;
using Kafka.DotNet.ksqlDB.Tests.Models;
using Kafka.DotNet.ksqlDB.Tests.Pocos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Visitors
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
    [Ignore("TODO")]
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
      kSqlFunction.Should().BeEquivalentTo("ENTRIES(MAP('a' := 'value'), true)");
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

    #region Collection functions
    
    private class Collection
    {
      public int[] Items1 { get; set; }
      public int[] Items2 { get; set; }
    }

    #region ArrayContains

    [TestMethod]
    public void ArrayContains_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, bool>> expression = c => K.Functions.ArrayContains(c.Items1, 2);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_CONTAINS({nameof(Collection.Items1)}, 2)");
    }

    [TestMethod]
    public void Array_BuildKSql_PrintsArrayFromProperties()
    {
      //Arrange
      Expression<Func<Tweet, int[]>> expression = c => new[] { c.Id, c.Id };

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY[{nameof(Tweet.Id)}, {nameof(Tweet.Id)}]");
    }

    #endregion

    #region ArrayDistinct
    
    [TestMethod]
    public void ArrayDistinct_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayDistinct(c.Items1);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_DISTINCT({nameof(Collection.Items1)})");
    }
    

    #endregion

    #region ArrayExcept
    
    [TestMethod]
    public void ArrayExcept_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayExcept(c.Items1, c.Items2);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_EXCEPT({nameof(Collection.Items1)}, {nameof(Collection.Items2)})");
    }

    #endregion

    #region ArrayIntersect

    [TestMethod]
    public void ArrayIntersect_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayIntersect(c.Items1, c.Items2);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_INTERSECT({nameof(Collection.Items1)}, {nameof(Collection.Items2)})");
    }    

    #endregion

    #region ArrayJoin

    [TestMethod]
    public void ArrayJoin_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, string>> expression = c => K.Functions.ArrayJoin(c.Items1, ";;");

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_JOIN({nameof(Collection.Items1)}, ';;')");
    }    

    #endregion

    #region ArrayRemove

    [TestMethod]
    public void ArrayRemove_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayRemove(c.Items1, 1);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_REMOVE({nameof(Collection.Items1)}, 1)");
    }    

    #endregion

    #region ArrayLength

    [TestMethod]
    public void ArrayLength_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int?>> expression = c => K.Functions.ArrayLength(c.Items1);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_LENGTH({nameof(Collection.Items1)})");
    }    

    #endregion

    #region ArrayMin

    [TestMethod]
    public void ArrayMin_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int?>> expression = c => K.Functions.ArrayMin(c.Items1);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_MIN({nameof(Collection.Items1)})");
    }    

    #endregion
    
    #region ArrayMax

    [TestMethod]
    public void ArrayMax_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int?>> expression = c => K.Functions.ArrayMax(c.Items1);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_Max({nameof(Collection.Items1)})");
    }    

    #endregion

    #region ArraySort
    
    [TestMethod]
    public void ArraySortNewArray_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int?[]>> expression = c => K.Functions.ArraySort(new int?[]{ 3, null, 1}, ListSortDirection.Ascending);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("ARRAY_SORT(ARRAY[3, NULL, 1], 'ASC')");
    } 

    [TestMethod]
    public void ArraySort_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int[]>> expression = c => K.Functions.ArraySort(c.Items1, ListSortDirection.Descending);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_SORT({nameof(Collection.Items1)}, 'DESC')");
    }    

    #endregion

    #region ArrayUnion
    
    [TestMethod]
    public void ArrayUnionNewArray_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int?[]>> expression = c => K.Functions.ArrayUnion(new int?[]{ 3, null, 1}, new int?[]{ 3, null});

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("ARRAY_UNION(ARRAY[3, NULL, 1], ARRAY[3, NULL])");
    } 

    [TestMethod]
    public void ArrayUnion_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, int[]>> expression = c => K.Functions.ArrayUnion(c.Items1, c.Items1);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"ARRAY_UNION({nameof(Collection.Items1)}, {nameof(Collection.Items1)})");
    }    

    #endregion

    #region AsMap
    
    [TestMethod]
    public void AsMap_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, IDictionary<string, int>>> expression = _ => K.Functions.AsMap(new []{ "1", "2" }, new []{ 11, 22 });

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("AS_MAP(ARRAY['1', '2'], ARRAY[11, 22])");
    } 

    #endregion

    #region JsonArrayContains
    
    [TestMethod]
    public void JsonArrayContains_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Collection, bool>> expression = _ => K.Functions.JsonArrayContains("[1, 2, 3]", 2);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo("JSON_ARRAY_CONTAINS('[1, 2, 3]', 2)");
    } 

    #endregion

    #endregion

    #region String functions

    #region Trim

    [TestMethod]
    public void Trim_BuildKSql_PrintsTrimFunction()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = c => K.Functions.Trim(c.Message);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"TRIM({nameof(Tweet.Message)})");
    }

    #endregion

    #region LPad
    
    [TestMethod]
    public void LPad_BuildKSql_PrintsLPadFunction()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = c => K.Functions.LPad(c.Message, 8, "x");

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"LPAD({nameof(Tweet.Message)}, 8, 'x')");
    }

    #endregion

    #region RPad
    
    [TestMethod]
    public void RPad_BuildKSql_PrintsRPadFunction()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = c => K.Functions.RPad(c.Message, 8, "x");

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"RPAD({nameof(Tweet.Message)}, 8, 'x')");
    }

    #endregion

    #region Substring
    
    [TestMethod]
    public void Substring_BuildKSql_PrintsSubstringFunction()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = c => K.Functions.Substring(c.Message, 2, 3);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"Substring({nameof(Tweet.Message)}, 2, 3)");
    }

    #endregion

    #region Like

    [TestMethod]
    public void Like_BuildKSql_PrintsLikeCondition()
    {
      //Arrange
      Expression<Func<Tweet, bool>> likeExpression = c => ksqlDB.KSql.Query.Functions.KSql.Functions.Like(c.Message, "santa%");

      //Act
      var query = ClassUnderTest.BuildKSql(likeExpression);

      //Assert
      query.Should().BeEquivalentTo($"{nameof(Tweet.Message)} LIKE 'santa%'");
    }

    [TestMethod]
    public void LikeToLower_BuildKSql_PrintsLikeCondition()
    {
      //Arrange
      Expression<Func<Tweet, bool>> likeExpression = c => ksqlDB.KSql.Query.Functions.KSql.Functions.Like(c.Message.ToLower(), "%santa%".ToLower());

      //Act
      var query = ClassUnderTest.BuildKSql(likeExpression);

      //Assert
      query.Should().BeEquivalentTo($"LCASE({nameof(Tweet.Message)}) LIKE LCASE('%santa%')");
    }

    #endregion    

    #region Concat

    [TestMethod]
    public void Concat_BuildKSql_PrintsTrimFunction()
    {
      //Arrange
      Expression<Func<Tweet, string>> expression = c => K.Functions.Concat(c.Message, "Value");

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo($"CONCAT({nameof(Tweet.Message)}, 'Value')");
    }

    #endregion

    #region Cast

    [TestMethod]
    public void ToInt_CastAsInt()
    {
      //Arrange
      Expression<Func<Tweet, int>> expression = c => Convert.ToInt32(c.Message);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS INT)");
    }

    [TestMethod]
    public void KSQLConvertToInt_CastAsInt()
    {
      //Arrange
      Expression<Func<Tweet, int>> expression = c => KSQLConvert.ToInt32(c.Message);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS INT)");
    }

    [TestMethod]
    public void ToLong_CastAsInt()
    {
      //Arrange
      Expression<Func<Tweet, long>> expression = c => Convert.ToInt64(c.Message);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS BIGINT)");
    }

    [TestMethod]
    public void KSQLConvertToLong_CastAsInt()
    {
      //Arrange
      Expression<Func<Tweet, long>> expression = c => KSQLConvert.ToInt64(c.Message);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS BIGINT)");
    }

    [TestMethod]
    public void ToDecimal_CastAsDecimal()
    {
      //Arrange
      Expression<Func<Tweet, decimal>> expression = c => KSQLConvert.ToDecimal(c.Message, 10, 2); //DECIMAL(precision, scale)

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS DECIMAL(10,2))");
    }

    [TestMethod]
    public void ToDecimal_CastAsDouble()
    {
      //Arrange
      Expression<Func<Tweet, double>> expression = c => Convert.ToDouble(c.Message);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS DOUBLE)");
    }

    [TestMethod]
    public void KSQLConvertToDecimal_CastAsDouble()
    {
      //Arrange
      Expression<Func<Tweet, double>> expression = c => KSQLConvert.ToDouble(c.Message);

      //Act
      var query = ClassUnderTest.BuildKSql(expression);

      //Assert
      query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS DOUBLE)");
    }

    #endregion
    
    #endregion

    #region Date and time functions

    [TestMethod]
    public void UnixDate_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Tweet, int>> expression = _ => K.Functions.UnixDate();

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo("UNIX_DATE()");
    }

    [TestMethod]
    public void UnixTimestamp_BuildKSql_PrintsFunction()
    {
      //Arrange
      Expression<Func<Tweet, long>> expression = _ => K.Functions.UnixTimestamp();

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo("UNIX_TIMESTAMP()");
    }

    [TestMethod]
    public void DateToString_BuildKSql_PrintsFunction()
    {
      //Arrange
      int epochDays = 18672;
      string format = "yyyy-MM-dd";
      Expression<Func<Tweet, string>> expression = _ => KSqlFunctions.Instance.DateToString(epochDays, format);

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo($"DATETOSTRING({epochDays}, '{format}')");
    }

    [TestMethod]
    public void StringToDate_BuildKSql_PrintsFunction()
    {
      //Arrange
      string formattedDate = "2021-02-17";
      string format = "yyyy-MM-dd";
      Expression<Func<Tweet, int>> expression = _ => KSqlFunctions.Instance.StringToDate(formattedDate, format);

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo($"STRINGTODATE('{formattedDate}', '{format}')");
    }

    [TestMethod]
    public void StringToTimestamp_BuildKSql_PrintsFunction()
    {
      //Arrange
      string formattedTimestamp = "2021-02-17";
      string format = "yyyy-MM-dd";
      Expression<Func<Tweet, long>> expression = _ => KSqlFunctions.Instance.StringToTimestamp(formattedTimestamp, format);

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo($"STRINGTOTIMESTAMP('{formattedTimestamp}', '{format}')");
    }

    [TestMethod]
    public void StringToTimestamp_TimeZone_BuildKSql_PrintsFunction()
    {
      //Arrange
      string formattedTimestamp = "2021-02-17";
      string format = "yyyy-MM-dd";
      string timeZone = "UTC";
      Expression<Func<Tweet, long>> expression = _ => KSqlFunctions.Instance.StringToTimestamp(formattedTimestamp, format, timeZone);

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo($"STRINGTOTIMESTAMP('{formattedTimestamp}', '{format}', '{timeZone}')");
    }

    [TestMethod]
    public void TimeStampToString_BuildKSql_PrintsFunction()
    {
      //Arrange
      long epochMilli = 1613503749145;
      string format = "yyyy-MM-dd HH:mm:ss.SSS";
      Expression<Func<Tweet, string>> expression = _ => KSqlFunctions.Instance.TimestampToString(epochMilli, format);

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo($"TIMESTAMPTOSTRING({epochMilli}, '{format}')");
    }

    [TestMethod]
    public void TimeStampToStringFormatWithBackTicks_BuildKSql_PrintsFunction()
    {
      //Arrange
      long epochMilli = 1613503749145;
      string format = "yyyy-MM-dd''T''HH:mm:ssX";
      Expression<Func<Tweet, string>> expression = _ => KSqlFunctions.Instance.TimestampToString(epochMilli, format);

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo($"TIMESTAMPTOSTRING({epochMilli}, '{format}')");
    }

    [TestMethod]
    public void TimeStampToStringWithTimeZone_BuildKSql_PrintsFunction()
    {
      //Arrange
      long epochMilli = 1613503749145;
      string format = "yyyy-MM-dd''T''HH:mm:ssX";
      string timeZone = "Europe/London";
      Expression<Func<Tweet, string>> expression = _ => KSqlFunctions.Instance.TimestampToString(epochMilli, format, timeZone);

      //Act
      var kSqlFunction = ClassUnderTest.BuildKSql(expression);

      //Assert
      kSqlFunction.Should().BeEquivalentTo($"TIMESTAMPTOSTRING({epochMilli}, '{format}', '{timeZone}')");
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