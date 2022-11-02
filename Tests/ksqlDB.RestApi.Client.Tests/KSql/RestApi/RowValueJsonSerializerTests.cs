using FluentAssertions;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi;

[TestClass]
public class RowValueJsonSerializerTests : TestBase
{
  private RowValueJsonSerializer ClassUnderTest { get; set; }

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    var queryStreamHeader = new QueryStreamHeader()
    {
      ColumnTypes = new []{ "STRING" },
      ColumnNames = new []{ "NAME" },
    };

    ClassUnderTest = new RowValueJsonSerializer(queryStreamHeader);
  }

  private record SingleLady
  {
    public string Name { get; set; }
  }

  [TestMethod]
  [Ignore]
  public void Deserialize_RecordWithSingleProperty()
  {
    //Arrange
    string rawJson = "[\"f03c278c-61ea-4f69-b153-5647d2eec72e\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<SingleLady>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Name.Should().Be("f03c278c-61ea-4f69-b153-5647d2eec72e");
  }

  [TestMethod]
  public void Deserialize_String()
  {
    //Arrange
    string rawJson = "[\"f03c278c-61ea-4f69-b153-5647d2eec72e\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<string>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be("f03c278c-61ea-4f69-b153-5647d2eec72e");
  }

  [TestMethod]
  public void Deserialize_Struct()
  {
    //Arrange
    var dateTime = new DateTimeOffset(new DateTime(2022, 9, 23), TimeSpan.FromHours(2));
    var dt = "2022-09-23T00:00:00+02:00";

    string rawJson = $"[\"{dt}\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<DateTimeOffset>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(dateTime);
  }

  [TestMethod]
  public void Deserialize_PrimitiveInt()
  {
    //Arrange
    var value = 42;

    string rawJson = $"[{value}]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<int>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(value);
  }

  private record MyStruct
  {
    public string Name { get; set; }
  }

  [TestMethod]
  [Ignore("TODO: single property classes")]
  public void Deserialize_MyStruct()
  {
    //Arrange
    string value = "E.T.";
    string rawJson = $"[\"{value}\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<MyStruct>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Name.Should().Be(value);
  }

  private enum MyEnum
  {
    None = 0
  }

  [TestMethod]
  public void Deserialize_Enum()
  {
    //Arrange
    var value = (int)MyEnum.None;

    string rawJson = $"[{value}]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<MyEnum>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(MyEnum.None);
  }

  [TestMethod]
  public void Deserialize_Guid()
  {
    //Arrange
    string guid = "f03c278c-61ea-4f69-b153-5647d2eec72e";
    string rawJson = $"[\"{guid}\"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    var rowValue = ClassUnderTest.Deserialize<Guid>(rawJson, jsonSerializationOptions);

    //Assert
    rowValue.Value.Should().Be(guid);
  }

  [TestMethod]
  [Ignore]
  public void Deserialize_Complex()
  {
    //Arrange
    string guid = "f03c278c-61ea-4f69-b153-5647d2eec72e";
    //string rawJson = $"[[{{\"A\":2}}]"]";
    var jsonSerializationOptions = KSqlDbJsonSerializerOptions.CreateInstance();

    //Act
    //var rowValue = ClassUnderTest.Deserialize<Guid>(rawJson, jsonSerializationOptions);

    //Assert
    //rowValue.Value.Should().Be(guid);
  }
}