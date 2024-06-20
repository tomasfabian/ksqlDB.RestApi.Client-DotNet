using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;
using UnitTests;
using Assert = NUnit.Framework.Assert;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Visitors;

public class KSqlFunctionVisitorStringTests : TestBase
{
  private ModelBuilder modelBuilder = null!;
  private KSqlFunctionVisitor ClassUnderTest { get; set; } = null!;

  private StringBuilder StringBuilder { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    modelBuilder = new();
    StringBuilder = new StringBuilder();
    ClassUnderTest = new KSqlFunctionVisitor(StringBuilder, new KSqlQueryMetadata { ModelBuilder = modelBuilder });
  }
    
  #region String functions

  #region Trim

  [Test]
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
    
  [Test]
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
    
  [Test]
  public void InstrOccurrence_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = c => K.Functions.Instr(c.Message, "sub", 1, 1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"INSTR({nameof(Tweet.Message)}, 'sub', 1, 1)");
  }
    
  [Test]
  public void InstrPosition_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = c => K.Functions.Instr(c.Message, "sub", 1);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"INSTR({nameof(Tweet.Message)}, 'sub', 1)");
  }
    
  [Test]
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
    
  [Test]
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
    
  [Test]
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

  [Test]
  public void Like_BuildKSql_PrintsLikeCondition()
  {
    //Arrange
    Expression<Func<Tweet, bool>> likeExpression = c => ksqlDB.RestApi.Client.KSql.Query.Functions.KSql.Functions.Like(c.Message, "santa%");

    //Act
    var query = ClassUnderTest.BuildKSql(likeExpression);

    //Assert
    query.Should().BeEquivalentTo($"{nameof(Tweet.Message)} LIKE 'santa%'");
  }

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
  public void FromBytes_BuildKSql_PrintsFunction()
  {
    //Arrange
    Expression<Func<Thumbnail, string>> expression = c => K.Functions.FromBytes(c.Image, "utf8");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo($"FROM_BYTES({nameof(Thumbnail.Image)}, 'utf8')");
  }

  [Test]
  public void FromBytes_CapturedVariable()
  {
    //Arrange
    byte[] bytes = Encoding.UTF8.GetBytes("Alien"); //QWxpZW4=

    Expression<Func<Thumbnail, string>> expression = c => K.Functions.FromBytes(bytes, "utf8");

    //Assert
    Assert.Throws<NotSupportedException>(() =>
    {
      //Act
      _ = ClassUnderTest.BuildKSql(expression);
    });
  }

  #endregion

  #region ConcatWS

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
  public void ToInt_CastAsInt()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = c => Convert.ToInt32(c.Message);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS INT)");
  }

  [Test]
  public void KSQLConvertToInt_CastAsInt()
  {
    //Arrange
    Expression<Func<Tweet, int>> expression = c => KSQLConvert.ToInt32(c.Message);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS INT)");
  }

  [Test]
  public void ToLong_CastAsInt()
  {
    //Arrange
    Expression<Func<Tweet, long>> expression = c => Convert.ToInt64(c.Message);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS BIGINT)");
  }

  [Test]
  public void KSQLConvertToLong_CastAsInt()
  {
    //Arrange
    Expression<Func<Tweet, long>> expression = c => KSQLConvert.ToInt64(c.Message);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS BIGINT)");
  }

  [Test]
  public void ToDecimal_CastAsDecimal()
  {
    //Arrange
    Expression<Func<Tweet, decimal>> expression = c => KSQLConvert.ToDecimal(c.Message, 10, 2); //DECIMAL(precision, scale)

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS DECIMAL(10,2))");
  }

  [Test]
  public void ToDecimal_CastAsDouble()
  {
    //Arrange
    Expression<Func<Tweet, double>> expression = c => Convert.ToDouble(c.Message);

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo(@$"CAST({nameof(Tweet.Message)} AS DOUBLE)");
  }

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
  public void JsonConcat_BuildKSql_PrintsTheFunction()
  {
    //Arrange
    Expression<Func<Tweet, string>> expression = c => K.Functions.JsonConcat("[1, 2]", "[3, 4]");

    //Act
    var query = ClassUnderTest.BuildKSql(expression);

    //Assert
    query.Should().BeEquivalentTo("JSON_CONCAT('[1, 2]', '[3, 4]')");
  }

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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
    public string name { get; set; } = null!;
  }

  [Test]
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
