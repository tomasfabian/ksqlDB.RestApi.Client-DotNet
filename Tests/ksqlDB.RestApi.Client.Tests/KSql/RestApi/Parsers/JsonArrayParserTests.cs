using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parsers;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Parsers;

public class JsonArrayParserTests : TestBase
{
  private JsonArrayParser ClassUnderTest { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = new JsonArrayParser();
  }

  [Test]
  public void CreateJson_PropertyNamesAreEqualToColumnHeaders()
  {
    //Arrange
    string[] headerColumns = { "KSQL_COL_0", "IsRobot" };
    string row = "{\"d\":4,\"c\":2},true";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    json.Should().NotBeEmpty();

    var jObject = JsonNode.Parse(json)!.AsObject();

    var property = jObject.First();
    property.Key.Should().BeEquivalentTo(headerColumns[0]);

    property = jObject.Last();
    property.Key.Should().BeEquivalentTo(headerColumns[1]);
  }

  [Test]
  public void CreateJson_PropertyValuesAreEqualToRowValues()
  {
    //Arrange
    string[] headerColumns = { "KSQL_COL_0", "IsRobot" };
    var mapValue = "{\"d\":4,\"c\":2}";
    string row = $"{mapValue}, true";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    var jObject = JsonNode.Parse(json)!.AsObject();

    var expectedJson = JsonNode.Parse(mapValue);
    var property = jObject.First();
    property.Value!.ToJsonString().Should().BeEquivalentTo(expectedJson!.ToJsonString());

    property = jObject.Last();
    property.Value!.ToJsonString().Should().BeEquivalentTo("true");
  }

  [Test]
  public void NestedArrayInMap()
  {
    //Arrange
    // {"queryId":"fd8ca155-a8e2-47f8-8cee-57fcd318a136","columnNames":["MAP"],"columnTypes":["MAP<STRING, ARRAY<INTEGER>>"]}
    // [{"a":[1,2],"b":[3,4]}]
    string[] headerColumns = { "MAP", "Value" };
    var arrayValue = "{\"a\":[1,2],\"b\":[3,4]}";
    string row = $"{arrayValue},1";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    var jObject = JsonNode.Parse(json)!.AsObject();

    var expectedJson = JsonNode.Parse(arrayValue);
    var property = jObject.First();
    property.Value!.ToJsonString().Should().BeEquivalentTo(expectedJson!.ToJsonString());

    property = jObject.Last();
    property.Value!.ToJsonString().Should().BeEquivalentTo("1");
  }

  [Test]
  public void NestedArrayInArray()
  {
    //Arrange
    string[] headerColumns = { "Value", "Arr" };
    var arrayValue = "[[1,2],[3,4]]";
    string row = $"1,{arrayValue}";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    json.Should().BeEquivalentTo($$"""{{{Environment.NewLine}}"Value": 1{{Environment.NewLine}},"Arr": [[1,2],[3,4]]{{Environment.NewLine}}}{{Environment.NewLine}}""");
  }

  [Test]
  public void NestedMapInArray()
  {
    //Arrange
    string[] headerColumns = { "Value", "Arr" };
    var arrayValue = "[{\"a\":1,\"b\":2},{\"d\":4,\"c\":3}]";
    string row = $"1,{arrayValue}";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    json.Should().BeEquivalentTo($$"""{{{Environment.NewLine}}"Value": 1{{Environment.NewLine}},"Arr": [{"a":1,"b":2},{"d":4,"c":3}]{{Environment.NewLine}}}{{Environment.NewLine}}""");
  }

  [Test]
  public void NestedMapInMap()
  {
    //Arrange
    string[] headerColumns = { "Value", "Map" };
    var mapValue = "{\"a\":{\"a\":1,\"b\":2},\"b\":{\"d\":4,\"c\":3}}";
    string row = $"1,{mapValue}";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    json.Should().BeEquivalentTo($$$"""{{{{Environment.NewLine}}}"Value": 1{{{Environment.NewLine}}},"Map": {"a":{"a":1,"b":2},"b":{"d":4,"c":3}}{{{Environment.NewLine}}}}{{{Environment.NewLine}}}""");
  }

  [Test]
  public void JsonWithCommaSeparatedValues()
  {
    //Arrange
    //{"queryId":"b3fd9708-c4c1-437c-9fc8-ae99ae72a1d7","columnNames":["ORDER_ID","NAME","CREATED_AT"],"columnTypes":["INTEGER","STRING","INTEGER"]}
    //[1,"Test1.8,8 ",18833]
    string[] headerColumns = { "ORDER_ID", "NAME", "CREATED_AT" };
    string row = "1,\"Test1.8,8\",18833";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    var jObject = JsonNode.Parse(json)!.AsObject();

    var nameProperty = jObject["NAME"];
    nameProperty!.GetValue<string>().Should().BeEquivalentTo("Test1.8,8");

    var expectedJson = JsonNode.Parse(
      @"{
""ORDER_ID"": 1,
""NAME"": ""Test1.8,8"",
""CREATED_AT"": 18833
}"
    );
    jObject.ToJsonString().Should().BeEquivalentTo(expectedJson!.ToJsonString());
  }

  [Test]
  public void Array_JsonWithCommaSeparatedValues()
  {
    //Arrange
    string[] headerColumns = { "Value", "Arr" };
    var arrayValue = "[[\"a\",\"b,c\"],[\"d,e\",\"f\"]]";
    string row = $"1,{arrayValue}";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    var expectedJson = JsonNode.Parse($"{{\"Value\": 1,\"Arr\": {arrayValue}}}");
    JsonNode.Parse(json)!.ToJsonString().Should().BeEquivalentTo(expectedJson!.ToJsonString());
  }

  [Test]
  public void CreateJson_Map_JsonWithCommaSeparatedValues()
  {
    //Arrange
    string[] headerColumns = { "KSQL_COL_0", "IsRobot" };
    var mapValue = "{\"d\":\"Test1.8,1\",\"c\":\"Test1.8,2\"}";
    string row = $"{mapValue}, true";

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    var expectedJson = JsonNode.Parse($"{{\"KSQL_COL_0\": {mapValue},\"IsRobot\": true}}");
    JsonNode.Parse(json)!.ToJsonString().Should().BeEquivalentTo(expectedJson!.ToJsonString());
  }

  [Test]
  public void CreateJson_QuoteInValue()
  {
    //Arrange
    string[] headerColumns = { "INTERNALID", "FILENAME", "TYPE", "NAME" };
    string row =
      "\"7775dee282f011724d3108f25302b999\",\"test.json\",\"Monitor\",\"27\\\" XZ272UVBMIIPHX Black\",\"\"";
    var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
    {
      WriteIndented = false,
    };

    //Act
    var json = ClassUnderTest.CreateJson(headerColumns, row);

    //Assert
    string expectedJson = JsonNode
      .Parse(
        $$$"""{"INTERNALID":"7775dee282f011724d3108f25302b999","FILENAME":"test.json","TYPE":"Monitor","NAME":"27\" XZ272UVBMIIPHX Black"}"""
      )
      .ToJsonString(options);
    var actual = JsonNode.Parse(json).ToJsonString(options);
    actual.Should().BeEquivalentTo(expectedJson);
  }

  private record IoTSensor
  {
    [Key]
    public string SensorId { get; set; } = null!;

    public int Value { get; set; }
  }

  private record IoTSensorChange : DatabaseChangeObject<IoTSensor>
  {
  }

  private record DatabaseChangeObject<TEntity> : DatabaseChangeObject
  {
    public TEntity? Before { get; set; }
    public TEntity? After { get; set; }
    public TEntity? EntityBefore => Before;
    public TEntity? EntityAfter => After;
  }

  private record DatabaseChangeObject
  {
    public string Op { get; set; } = null!;
    public long? TsMs { get; set; }
  }

  [Test]
  public void ValueWithSquareBrackets()
  {
    //Arrange
    string[] headerColumns = { "BEFORE", "AFTER", "OP", "TSMS" };
    string row = "{\"SENSORID\":\"f575a8a9-e]\",\"VALUE\":88},{\"SENSORID\":\"f575a8a9-e\",\"VALUE\":58},\"u\",null";

    var jsonRecord = ClassUnderTest.CreateJson(headerColumns, row);

    JsonSerializerOptions jsonSerializerOptions = new()
    {
      PropertyNameCaseInsensitive = true,
    };

    //Act
    var record = JsonSerializer.Deserialize<IoTSensorChange>(jsonRecord, jsonSerializerOptions);

    //Assert
    record.Should().NotBeNull();
    record!.Before!.SensorId.Should().Be("f575a8a9-e]");
  }
}
