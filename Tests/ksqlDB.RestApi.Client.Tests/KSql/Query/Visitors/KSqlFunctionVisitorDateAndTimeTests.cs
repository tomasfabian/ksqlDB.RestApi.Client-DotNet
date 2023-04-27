using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using NUnit.Framework;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

public class KSqlFunctionVisitorDateAndTimeTests : TestBase
{
  private KSqlFunctionVisitor ClassUnderTest { get; set; } = null!;

  private StringBuilder StringBuilder { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlFunctionVisitor(StringBuilder, new KSqlQueryMetadata());
  }

  #region Date and time functions

  [Test]
  public void UnixDate_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = _ => K.Functions.UnixDate();

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo("UNIX_DATE()");
  }

  [Test]
  public void UnixTimestamp_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, long>> expression = _ => K.Functions.UnixTimestamp();

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo("UNIX_TIMESTAMP()");
  }

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
  public void FormatDate_BuildKSql_PrintsFunction()
  {
    //Arrange
    DateTime date = new DateTime(2022,4, 11);
    string format = "yyyy-MM-dd''T''HH:mm:ssX";
    Expression<Func<Tweet, string>> expression = _ => KSqlFunctions.Instance.FormatDate(date, format);

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo($"FORMAT_DATE('2022-04-11', '{format}')");
  }

  [Test]
  public void FormatTime_BuildKSql_PrintsFunction()
  {
    //Arrange
    TimeSpan time = new TimeSpan(10, 1, 22);
    string format = "''T''HH:mm:ssX";
    Expression<Func<Tweet, string>> expression = _ => KSqlFunctions.Instance.FormatTime(time, format);

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo($"FORMAT_TIME('10:01:22', '{format}')");
  }

  [Test]
  public void ParseDate_BuildKSql_PrintsFunction()
  {
    //Arrange
    string formattedDate = "2022-04-16";
    string format = "yyyy-MM-dd''T''";
    Expression<Func<Tweet, DateTime>> expression = _ => KSqlFunctions.Instance.ParseDate(formattedDate, format);

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo($"PARSE_DATE('{formattedDate}', '{format}')");
  }

  [Test]
  public void ParseTime_BuildKSql_PrintsFunction()
  {
    //Arrange
    string formattedTime = "10:01:22";
    string format = "''T''HH:mm:ssX";
    Expression<Func<Tweet, TimeSpan>> expression = _ => KSqlFunctions.Instance.ParseTime(formattedTime, format);

    //Act
    var kSqlFunction = ClassUnderTest.BuildKSql(expression);

    //Assert
    kSqlFunction.Should().BeEquivalentTo($"PARSE_TIME('{formattedTime}', '{format}')");
  }

  #endregion
}
