using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Visitors;

[TestClass]
public class KSqlFunctionVisitorStringTests : TestBase
{
  private KSqlFunctionVisitor ClassUnderTest { get; set; }

  private StringBuilder StringBuilder { get; set; }

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlFunctionVisitor(StringBuilder, new KSqlQueryMetadata());
  }
    
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

  #region Instr
    
  [TestMethod]
  public void InstrOccurrence_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = c => K.Functions.Instr(c.Message, "sub", 1, 1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"INSTR({nameof(Tweet.Message)}, 'sub', 1, 1)");
  }
    
  [TestMethod]
  public void InstrPosition_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = c => K.Functions.Instr(c.Message, "sub", 1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"INSTR({nameof(Tweet.Message)}, 'sub', 1)");
  }
    
  [TestMethod]
  public void Instr_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = c => K.Functions.Instr(c.Message, "sub");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"INSTR({nameof(Tweet.Message)}, 'sub')");
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
    Expression<Func<Tweet, bool>> likeExpression = c => ksqlDB.RestApi.Client.KSql.Query.Functions.KSql.Functions.Like(c.Message, "santa%");

    //Act
    var query = ClassUnderTest.BuildKSql(likeExpression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Tweet.Message)} LIKE 'santa%'");
  }

  [TestMethod]
  public void LikeToLower_BuildKSql_PrintsLikeCondition()
  {
    //Arrange
    Expression<Func<Tweet, bool>> likeExpression = c => ksqlDB.RestApi.Client.KSql.Query.Functions.KSql.Functions.Like(c.Message.ToLower(), "%santa%".ToLower());

    //Act
    var query = ClassUnderTest.BuildKSql(likeExpression);

    //Assert
    query.Should().BeEquivalentTo($"LCASE({nameof(Tweet.Message)}) LIKE LCASE('%santa%')");
  }

  #endregion    

  #region Concat

  [TestMethod]
  public void Concat_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = c => K.Functions.Concat(c.Message, "Value");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"CONCAT({nameof(Tweet.Message)}, 'Value')");
  }

  #endregion

  #region ToBytes

  [TestMethod]
  public void ToBytes_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, byte[]>> expression = c => K.Functions.ToBytes(c.Message, "utf8");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"TO_BYTES({nameof(Tweet.Message)}, 'utf8')");
  }

  #endregion

  #region FromBytes

  struct Thumbnail
  {
    public byte[] Image { get; init; }
  }

  [TestMethod]
  public void FromBytes_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Thumbnail, string>> expression = c => K.Functions.FromBytes(c.Image, "utf8");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"FROM_BYTES({nameof(Thumbnail.Image)}, 'utf8')");
  }

  [TestMethod]
  [ExpectedException(typeof(NotSupportedException))]
  public void FromBytes_CapturedVariable()
  {
    //Arrange
    byte[] bytes = Encoding.UTF8.GetBytes("Alien");
    //QWxpZW4=

    Expression<Func<Thumbnail, string>> expression = c => K.Functions.FromBytes(bytes, "utf8");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"FROM_BYTES({nameof(Thumbnail.Image)}, 'utf8')");
  }

  #endregion

  #region ConcatWS

  [TestMethod]
  public void ConcatWS_BuildKSql_PrintsFunction()
  {
    //Arrange
    string separator = " - ";
    Expression<Func<Tweet, string>> expression = c => K.Functions.ConcatWS(separator, c.Message, "Value");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"CONCAT_WS(' - ', {nameof(Tweet.Message)}, 'Value')");
  }

  #endregion

  #region Encode

  [TestMethod]
  public void Encode_BuildKSql_PrintsFunction()
  {
    //Arrange
    string inputEncoding = "utf";
    string outputEncoding = "ascii";
    Expression<Func<Tweet, string>> expression = c => K.Functions.Encode(c.Message, inputEncoding, outputEncoding);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"ENCODE({nameof(Tweet.Message)}, '{inputEncoding}', '{outputEncoding}')");
  }

  #endregion

  #region InitCap

  [TestMethod]
  public void InitCap_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = c => K.Functions.InitCap(c.Message);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"INITCAP({nameof(Tweet.Message)})");
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

  #region Json

  #region ExtractJsonField

  [TestMethod]
  public void ExtractJsonField_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    string jsonPath = "$.log.cloud";
    Expression<Func<Tweet, string>> expression = c => K.Functions.ExtractJsonField(c.Message, jsonPath);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"EXTRACTJSONFIELD({nameof(Tweet.Message)}, '{jsonPath}')");
  }

  #endregion

  #region IsJsonString

  [TestMethod]
  public void IsJsonString_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    string jsonInput = "{}";
    Expression<Func<Tweet, bool>> expression = c => K.Functions.IsJsonString(jsonInput);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"IS_JSON_STRING('{jsonInput}')");
  }

  #endregion

  #region JsonArrayLength

  [TestMethod]
  public void JsonArrayLength_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    string jsonInput = "[1, 2, 3]";
    Expression<Func<Tweet, int?>> expression = c => K.Functions.JsonArrayLength(jsonInput);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"JSON_ARRAY_LENGTH('{jsonInput}')");
  }

  [TestMethod]
  public void JsonArrayLength_Null_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    string jsonInput = null;
    Expression<Func<Tweet, int?>> expression = c => K.Functions.JsonArrayLength(jsonInput);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("JSON_ARRAY_LENGTH(NULL)");
  }

  [TestMethod]
  public void JsonConcat_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = c => K.Functions.JsonConcat("[1, 2]", "[3, 4]");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("JSON_CONCAT('[1, 2]', '[3, 4]')");
  }

  [TestMethod]
  public void JsonKeys_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    string jsonInput = "[1, 2, 3]";
    Expression<Func<Tweet, string[]>> expression = c => K.Functions.JsonKeys(jsonInput);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"JSON_KEYS('{jsonInput}')");
  }

  [TestMethod]
  public void JsonRecords_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    string jsonInput = "{\"a\": \"abc\", \"b\": { \"c\": \"a\" }, \"d\": 1}";
    Expression<Func<Tweet, IDictionary<string, string>>> expression = c => K.Functions.JsonRecords(jsonInput);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"JSON_RECORDS('{jsonInput}')");
  }

  [TestMethod]
  public void ToJsonString_Bool_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    var input = true;
    Expression<Func<Tweet, string>> expression = c => K.Functions.ToJsonString(input);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"TO_JSON_STRING({input})");
  }

  [TestMethod]
  public void ToJsonString_String_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    var input = "abc";
    Expression<Func<Tweet, string>> expression = c => K.Functions.ToJsonString(input);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"TO_JSON_STRING('{input}')");
  }

  [TestMethod]
  public void ToJsonString_Array_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    var input = new[] { 1,2,3 };
    Expression<Func<Tweet, string>> expression = c => K.Functions.ToJsonString(input);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("TO_JSON_STRING(Array[1, 2, 3])");
  }

  [TestMethod]
  public void ToJsonString_Dictionary_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    var input = new Dictionary<string, int>
    {
      ["c"] = 2, 
      ["d"] = 4
    };

    Expression<Func<Tweet, string>> expression = c => K.Functions.ToJsonString(input);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("TO_JSON_STRING(Map('c' := 2, 'd' := 4))");
  }

  private class MyType
  {
    public int id { get; set; }
    public string name { get; set; }
  }

  [TestMethod]
  public void ToJsonString_Struct_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    var input = new MyType
    {
      id = 1,
      name = "A"
    };

    Expression<Func<Tweet, string>> expression = c => K.Functions.ToJsonString(input);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("TO_JSON_STRING(STRUCT(id := 1, name := 'A'))");
  }

  #endregion

  #endregion

  #endregion
}