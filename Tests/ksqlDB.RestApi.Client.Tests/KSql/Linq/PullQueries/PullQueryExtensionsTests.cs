using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.Tests.Models;
using ksqlDb.RestApi.Client.Tests.Models.Sensors;
using NUnit.Framework;
using UnitTests;
using static ksqlDB.RestApi.Client.KSql.RestApi.Enums.IdentifierEscaping;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.Linq.PullQueries;

public class PullQueryExtensionsTests : TestBase
{
  private TestableDbProvider DbProvider { get; set; } = null!;

  protected virtual string PullQueryResponse { get; set; } = @"[{""header"":{""queryId"":""query_1619945627639"",""schema"":""`SENSORID` STRING KEY, `WINDOWSTART` BIGINT KEY, `WINDOWEND` BIGINT KEY, `AVGVALUE` DOUBLE""}},
{""row"":{""columns"":[""sensor-1"",1619859745000,1619859750000,11.0]}},";

  [SetUp]
  public void SetUp()
  {
    DbProvider = new TestableDbProvider(TestParameters.KsqlDbUrl, PullQueryResponse);
    DbProvider.ContextOptions.EndpointType = EndpointType.Query;
  }

  [Test]
  public async Task CreatePullQuery()
  {
    //Arrange
    string sensorId = "sensor-1";

    DbProvider.RegisterKSqlDbProvider = false;

    //Act
    var result = await DbProvider.CreatePullQuery<IoTSensorStats>()
      .Where(c => c.SensorId == sensorId)
      .FirstOrDefaultAsync();
      
    //Assert
    result.Should().NotBeNull();
    result!.SensorId.Should().Be(sensorId);
  }

  [Test]
  public void SelectColumns()
  {
    //Arrange
    string sensorId = "sensor-1";

    //Act
    var ksql = DbProvider.CreatePullQuery<IoTSensorStats>()
      .Where(c => c.SensorId == sensorId)
      .Select(c => new { c.SensorId, Avg = c.AvgValue })
      .ToQueryString();
      
    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT {nameof(IoTSensorStats.SensorId)}, AvgValue AS Avg FROM {nameof(IoTSensorStats)}
WHERE SensorId = '{sensorId}';".ReplaceLineEndings());
  }

  [Test]
  public void CreatePullQuery_ToQueryString()
  {
    //Arrange
    string sensorId = "sensor-1";      
    var query = DbProvider.CreatePullQuery<IoTSensorStats>()
      .Where(c => c.SensorId == sensorId);

    //Act
    var ksql1 = query.ToQueryString();
    var ksql2 = query.ToQueryString();

    //Assert
    ksql1.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(IoTSensorStats)}
WHERE SensorId = '{sensorId}';".ReplaceLineEndings());

    ksql1.Should().BeEquivalentTo(ksql2);
  }

  private const string MaterializedViewName = "TestViews";

  [Test]
  public void CreatePullQuery_TableNameWasOverriden()
  {
    //Arrange
    string sensorId = "sensor-1";      
    var query = DbProvider.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
      .Where(c => c.SensorId == sensorId);

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {MaterializedViewName}
WHERE SensorId = '{sensorId}';".ReplaceLineEndings());
  }

  [Test]
  public void CreatePullQuery_WindowBounds_LongTypeDateFormat()
  {
    //Arrange
    string sensorId = "sensor-1";      
    var query = DbProvider.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
      .Where(c => c.SensorId == sensorId);

    long windowStart = 1575044700000;
    long windowEnd = 1675044700000;

    //Act
    var ksql = query.Where(c => Bounds.WindowStart > windowStart && Bounds.WindowEnd <= windowEnd).ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {MaterializedViewName}
WHERE SensorId = '{sensorId}' AND (WINDOWSTART > {windowStart}) AND (WINDOWEND <= {windowEnd});".ReplaceLineEndings());
  }

  [Test]
  public void CreatePullQuery_WindowBounds_StringTypeDateFormat()
  {
    //Arrange
    string sensorId = "sensor-1";      
    var query = DbProvider.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
      .Where(c => c.SensorId == sensorId);

    string windowStart = "2019-10-03T21:31:16";
    string windowEnd = "2020-10-03T21:31:16";

    //Act
    var ksql = query.Where(c => Bounds.WindowStart > windowStart && Bounds.WindowEnd <= windowEnd).ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {MaterializedViewName}
WHERE SensorId = '{sensorId}' AND (WINDOWSTART > '{windowStart}') AND (WINDOWEND <= '{windowEnd}');".ReplaceLineEndings());
  }

  [Test]
  public void Take()
  {
    //Arrange
    int limit = 5;

    //Act
    var ksql = DbProvider.CreatePullQuery<IoTSensorStats>(MaterializedViewName)
      .Take(limit)
      .ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo($"SELECT * FROM {MaterializedViewName} LIMIT {limit};");
  }

  [TestCase(Never, ExpectedResult = $"SELECT sub FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Keywords, ExpectedResult = $"SELECT sub FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Always, ExpectedResult = $"SELECT `sub` FROM `{nameof(JsonPropertyNameTestData)}`;")]
  public string SelectColumnsUsingPullQueryThatHaveJsonPropertyName(IdentifierEscaping escaping)
  {
    //Arrange
    var dbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });

    //Act
    var ksql = dbContext.CreatePullQuery<JsonPropertyNameTestData>()
      .Select(c => new { c.SubProperty })
      .ToQueryString();

    //Assert
    return ksql.ReplaceLineEndings();
  }

  [TestCase(Never, ExpectedResult = $"SELECT sub AS Prop FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Keywords, ExpectedResult = $"SELECT sub AS Prop FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Always, ExpectedResult = $"SELECT `sub` AS `Prop` FROM `{nameof(JsonPropertyNameTestData)}`;")]
  public string SelectColumnsUsingPullQueryThatHaveJsonPropertyNameWithAlias(IdentifierEscaping escaping)
  {
    //Arrange
    var dbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });

    //Act
    var ksql = dbContext.CreatePullQuery<JsonPropertyNameTestData>()
      .Select(c => new { Prop = c.SubProperty })
      .ToQueryString();

    //Assert
    return ksql.ReplaceLineEndings();
  }

  [TestCase(Never, ExpectedResult = $"SELECT RowTime, sub FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Keywords, ExpectedResult = $"SELECT RowTime, sub FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Always, ExpectedResult = $"SELECT RowTime, `sub` FROM `{nameof(JsonPropertyNameTestData)}`;")]
  public string SelectColumnsUsingPullQueryThatHaveJsonPropertyNameWithPseudoColumn(IdentifierEscaping escaping)
  {
    //Arrange
    var dbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });

    //Act
    var ksql = dbContext.CreatePullQuery<JsonPropertyNameTestData>()
      .Select(c => new { c.RowTime, c.SubProperty })
      .ToQueryString();

    //Assert
    return ksql.ReplaceLineEndings();
  }

  [TestCase(Never, ExpectedResult = $"SELECT RowTime, sub AS Prop FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Keywords, ExpectedResult = $"SELECT RowTime, sub AS Prop FROM {nameof(JsonPropertyNameTestData)};")]
  [TestCase(Always, ExpectedResult = $"SELECT RowTime, `sub` AS `Prop` FROM `{nameof(JsonPropertyNameTestData)}`;")]
  public string SelectColumnsUsingPullQueryThatHaveJsonPropertyNameWithAliasWithPseudoColum(IdentifierEscaping escaping)
  {
    //Arrange
    var dbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      { IdentifierEscaping = escaping });

    //Act
    var ksql = dbContext.CreatePullQuery<JsonPropertyNameTestData>()
      .Select(c => new { c.RowTime, Prop = c.SubProperty })
      .ToQueryString();

    //Assert
    return ksql.ReplaceLineEndings();
  }

  [TestCase(Never, ExpectedResult = @$"SELECT prop FROM {nameof(SubProperty)}
WHERE prop = 'test';")]
  [TestCase(Keywords, ExpectedResult = @$"SELECT prop FROM {nameof(SubProperty)}
WHERE prop = 'test';")]
  [TestCase(Always, ExpectedResult = @$"SELECT `prop` FROM `{nameof(SubProperty)}`
WHERE `prop` = 'test';")]
  public string SelectColumnsUsingPullQueryThatHaveJsonPropertyNameAndWhere(IdentifierEscaping escaping)
  {
    //Arrange
    var dbContext = new KSqlDBContext(new KSqlDBContextOptions(TestParameters.KsqlDbUrl)
      {IdentifierEscaping = escaping, ShouldPluralizeFromItemName = false});

    //Act
    var ksql = dbContext.CreatePullQuery<SubProperty>()
      .Select(c => new {c.Property})
      .Where(c => c.Property == "test")
      .ToQueryString();

    //Assert
    return ksql.ReplaceLineEndings();
  }
}
