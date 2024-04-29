using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Formats;
using ksqlDb.RestApi.Client.Tests.KSql.Linq;
using ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements;
using ksqlDb.RestApi.Client.Tests.Models;
using NUnit.Framework;
using UnitTests;
using Location = ksqlDb.RestApi.Client.Tests.Models.Location;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query;

#pragma warning disable CA1861

public class KSqlQueryLanguageVisitorTests : TestBase
{
  private KSqlQueryGenerator ClassUnderTest { get; set; } = null!;

  readonly string streamName = nameof(Location) + "s";

  private KSqlDBContextOptions contextOptions = null!;
  private QueryContext queryContext = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDbUrl);
    queryContext = new QueryContext();
    ClassUnderTest = new KSqlQueryGenerator(contextOptions);
  }

  private IQbservable<Location> CreateStreamSource(bool shouldPluralizeStreamName = true)
  {
    contextOptions.ShouldPluralizeFromItemName = shouldPluralizeStreamName;

    var context = new TestableDbProvider(contextOptions);

    return context.CreatePushQuery<Location>();
  }

  private IQbservable<Tweet> CreateTweetsStreamSource()
  {
    var context = new TestableDbProvider(contextOptions);

    return context.CreatePushQuery<Tweet>();
  }

  #region Select

  public class MySensor
  {
    [JsonPropertyName("SensorId")]
    public string SensorId2 { get; set; } = null!;

    public string Data { get; set; } = null!;

    [JsonPropertyName("data_id")]
    public string DataId { get; set; } = null!;
  }

  [Test]
  public void BuildKSql_SelectPropertyWithJsonPropertyNameAttribute()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<MySensor>()
      .Where(c => c.SensorId2 == "1")
      .Select(c => c.SensorId2);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT SensorId FROM {nameof(MySensor)}s
WHERE SensorId = '1' EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql.ReplaceLineEndings());
  }

  [Test]
  public void BuildKSql_SelectFromJoinPropertyWithJsonPropertyNameAttribute()
  {
    //Arrange
    contextOptions.ShouldPluralizeFromItemName = false;
    string stream1TableName = "stream1";
    string stream2TableName = "stream2";

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<MySensor>(stream1TableName)
      .Join(Source.Of<MySensor>(stream2TableName).Within(Duration.OfDays(1)),
        endUsers => K.Functions.ExtractJsonField(endUsers.Data, "$.customer_id"),
        transactions => K.Functions.ExtractJsonField(transactions.Data, "$.customer_id"),
        (endUsers, transactions) => new
        {
          EnduserId = endUsers.DataId,
          TransactionsId = transactions.DataId,
          CustomerId = K.Functions.ExtractJsonField(endUsers.Data, "$.customer_id"),
          EndusersData = endUsers.Data,
          TransactionsData = transactions.Data
        });

    queryContext.FromItemName = "stream1";

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT endusers.data_id AS EnduserId, transactions.data_id AS TransactionsId, EXTRACTJSONFIELD(endusers.Data, '$.customer_id') CustomerId, endusers.Data AS EndusersData, transactions.Data AS TransactionsData FROM {stream1TableName} endusers
INNER JOIN {stream2TableName} transactions
WITHIN 1 DAYS ON EXTRACTJSONFIELD(endusers.Data, '$.customer_id') = EXTRACTJSONFIELD(transactions.Data, '$.customer_id')
EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void BuildKSql_SelectFromMultiJoinPropertyWithJsonPropertyNameAttribute()
  {
    //Arrange
    contextOptions.ShouldPluralizeFromItemName = false;
    string stream1TableName = "stream1";
    string stream2TableName = "stream2";

    var query = (from a in new TestableDbProvider(contextOptions).CreatePushQuery<MySensor>(stream1TableName)
      join b in Source.Of<MySensor>(stream2TableName) on a.DataId equals b.DataId
      select new
      {
        a.DataId
      });

    queryContext.FromItemName = "stream1";

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT a.data_id DataId FROM {stream1TableName} a
INNER JOIN {stream2TableName} b
ON a.data_id = b.data_id
EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  [Ignore("TODO")]
  public void BuildKSql_SelectFrom3MultiJoinSameTypePropertyWithJsonPropertyNameAttribute()
  {
    //Arrange
    contextOptions.ShouldPluralizeFromItemName = false;

    string stream1TableName = "stream1";
    string stream2TableName = "stream2";
    string stream3TableName = "stream3";

    var query = (from a in new TestableDbProvider(contextOptions).CreatePushQuery<MySensor>(stream1TableName)
      join b in Source.Of<MySensor>(stream2TableName) on a.DataId equals b.DataId
      join c in Source.Of<MySensor>(stream3TableName) on a.DataId equals c.DataId
      select new
      {
        a.DataId,
        b.Data,
        c.SensorId2,
      });

    queryContext.FromItemName = "stream1";

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT a.data_id DataId, b.Data Data, c.SensorId SensorId2 FROM {stream1TableName} a
INNER JOIN {stream3TableName} c
ON a.data_id = c.data_id
INNER JOIN {stream2TableName} b
ON a.data_id = b.data_id
EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectAlias_BuildKSql_PrintsProjection()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(l => new { Lngt = l.Longitude, l.Latitude });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(Location.Longitude)} AS Lngt, {nameof(Location.Latitude)} FROM {streamName} EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectTwoAliasesWithBinaryOperations_BuildKSql_PrintsProjection()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(l => new { Lngt = l.Longitude / 2, Lat = l.Latitude + "" });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(Location.Longitude)} / 2 AS Lngt, {nameof(Location.Latitude)} + '' AS Lat FROM {streamName} EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectWhere_BuildKSql_PrintsSelectFromWhere()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(l => new { l.Longitude, l.Latitude })
      .Where(p => p.Latitude == "1");

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT {nameof(Location.Longitude)}, {nameof(Location.Latitude)} FROM {streamName}
WHERE {nameof(Location.Latitude)} = '1' EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectMultipleWhere_BuildKSql_PrintsSelectFromWheres()
  {
    //Arrange
    var query = CreateStreamSource()
      .Where(p => p.Latitude == "1")
      .Where(p => p.Longitude == 0.1)
      .Select(l => new { l.Longitude, l.Latitude });

    KSqlDBContextOptions.NumberFormatInfo = new System.Globalization.NumberFormatInfo
    {
      NumberDecimalSeparator = "."
    };

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT {nameof(Location.Longitude)}, {nameof(Location.Latitude)} FROM {streamName}
WHERE {nameof(Location.Latitude)} = '1' AND {nameof(Location.Longitude)} = 0.1 EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectDynamicFunction_BuildKSql_PrintsFunctionCall()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { c.Longitude, c.Latitude, Col = ksqlDB.RestApi.Client.KSql.Query.Functions.KSql.F.Dynamic("IFNULL(Latitude, 'n/a')") as string });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(Location.Longitude)}, {nameof(Location.Latitude)}, IFNULL(Latitude, 'n/a') Col FROM {streamName} EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectListMember_BuildKSql_PrintsArray()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Select(l => new List<int> { 1, 3 });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql = $"SELECT ARRAY[1, 3] FROM {nameof(OrderData)} EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectList_BuildKSql_PrintsArray()
  {
    //Arrange
    var orderTypes = new List<int> { 1, 3 };

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Select(l => new { OrderTypes = orderTypes });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql = $"SELECT ARRAY[1, 3] AS OrderTypes FROM {nameof(OrderData)} EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectListContains_BuildKSql_PrintsIn()
  {
    //Arrange
    var orderTypes = new List<int> { 1, 3 };

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Select(c => orderTypes.Contains(c.OrderType));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo($"SELECT {nameof(OrderData.OrderType)} IN (1, 3) FROM {nameof(OrderData)} EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void SelectNewListContains_BuildKSql_PrintsIn()
  {
    //Arrange
    var orderTypes = new List<int> { 1, 3 };

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Select(c => new { Contains = new List<int> { 1, 3 }.Contains(c.OrderType) });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo($"SELECT {nameof(OrderData.OrderType)} IN (1, 3) Contains FROM {nameof(OrderData)} EMIT CHANGES;".ReplaceLineEndings());
  }

  #region CapturedVariables

  [Test]
  public void Transform_CapturedNestedPropertyAccessor()
  {
    //Arrange
    var value = new Dictionary<string, IDictionary<string, int>>()
    {
      { "a", new Dictionary<string, int>() { { "a", 1 }, { "b", 2 } } },
      { "b", new Dictionary<string, int>() { { "a", 3 }, { "d", 4 } } }
    };

    var query = CreateStreamSource().Select(_ => new
    {
      Dict = K.Functions.Transform(value, (k, v) => k.ToUpper(), (k, v) => v["a"] + 1)
    });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be($"SELECT TRANSFORM(MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('a' := 3, 'd' := 4)), (k, v) => UCASE(k), (k, v) => v['a'] + 1) Dict FROM {streamName} EMIT CHANGES;".ReplaceLineEndings());
  }

  struct Foo
  {
    public int Prop { get; set; }
  }

  [Test]
  public void Select_CapturedStructWithAlias()
  {
    //Arrange
    var value = new Foo { Prop = 42 };

    var query = CreateStreamSource().Select(_ => new
    {
      C = value
    });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be($"SELECT STRUCT(Prop := 42) AS C FROM {streamName} EMIT CHANGES;".ReplaceLineEndings());
  }

  class FooClass
  {
    public int Prop { get; set; }
  }

  [Test]
  public void Select_CapturedClassWithAlias()
  {
    //Arrange
    var value = new FooClass { Prop = 42 };

    var query = CreateStreamSource().Select(_ => new
    {
      C = value
    });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be($"SELECT STRUCT(Prop := 42) AS C FROM {streamName} EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void Select_CapturedStruct()
  {
    //Arrange
    var value = new Foo { Prop = 42 };

    var query = CreateStreamSource().Select(_ => new
    {
      value
    });

    //Act
    var ksql = query.ToQueryString();

    //Assert
    ksql.Should().Be($"SELECT STRUCT(Prop := 42) FROM {streamName} EMIT CHANGES;".ReplaceLineEndings());
  }

  #endregion

  #region Time types

  [Test]
  public void Select_TimeField_PrintsBetween()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Select(p => new DateTimeOffset(2021, 7, 4, 13, 29, 45, 447, TimeSpan.FromHours(4)));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      "SELECT '2021-07-04T13:29:45.447+04:00' FROM TimeTypes EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void Select_CapturedTimeVariable_PrintsBetween()
  {
    //Arrange
    var dateTimeOffset = new DateTimeOffset(2021, 7, 4, 13, 29, 45, 447, TimeSpan.FromHours(4));

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Select(p => dateTimeOffset);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      "SELECT '2021-07-04T13:29:45.447+04:00' FROM TimeTypes EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void Select_NewTimeVariable_PrintsBetween()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Select(p => new { Time = new DateTime(2021, 4, 1) });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      "SELECT '2021-04-01' Time FROM TimeTypes EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void Select_MinValue_PrintsBetween()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Where(c => c.Dt.Between(DateTime.MinValue, DateTime.MaxValue));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @"SELECT * FROM TimeTypes
WHERE Dt BETWEEN '0001-01-01' AND '9999-12-31' EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void Select_DateTimeNow_PrintsBetween()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Select(c => DateTime.Now);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT '{DateTime.Now.ToString(ValueFormats.DateFormat)}' FROM TimeTypes EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().Be(expectedKsql);
  }

  [Test]
  public void Select_CapturedDateTime_PrintsBetween()
  {
    //Arrange
    var from = new DateTime(2021, 10, 1);

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Select(c => from);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      "SELECT '2021-10-01' FROM TimeTypes EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void Select_CapturedDateTimeNew_PrintsBetween()
  {
    //Arrange
    var from = new DateTime(2021, 10, 1);

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Select(c => new { Ts = from, DateTime.MinValue });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      "SELECT '2021-10-01' AS Ts, '0001-01-01' MinValue FROM TimeTypes EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }
    
  #endregion

  #endregion

  #region Where

  [Test]
  public void Where_BuildKSql_PrintsWhere()
  {
    //Arrange
    var query = CreateStreamSource()
      .Where(p => p.Latitude == "1");

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {streamName}
WHERE {nameof(Location.Latitude)} = '1' EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void WhereStringsEquals_BuildKSql_PrintsWhere()
  {
    //Arrange
    var query = CreateStreamSource()
      .Where(p => p.Latitude.Equals("1"));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {streamName}
WHERE {nameof(Location.Latitude)} = '1' EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void WhereDoublesEquals_BuildKSql_PrintsWhere()
  {
    //Arrange
    var query = CreateStreamSource()
      .Where(p => p.Longitude.Equals(1.1));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {streamName}
WHERE {nameof(Location.Longitude)} = 1.1 EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void WhereSelect_BuildKSql_PrintsSelectFromWhere()
  {
    //Arrange
    var query = CreateStreamSource()
      .Where(p => p.Latitude == "1")
      .Select(l => new { l.Longitude, l.Latitude });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT {nameof(Location.Longitude)}, {nameof(Location.Latitude)} FROM {streamName}
WHERE {nameof(Location.Latitude)} = '1' EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  class OrderData
  {
    public int OrderType { get; set; }
    public string Category { get; init; } = null!;
  }

  [Test]
  public void WhereContains_BuildKSql_PrintsArrayContains()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Where(c => K.Functions.ArrayContains(new[] { 1, 3 }, c.OrderType));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE ARRAY_CONTAINS(ARRAY[1, 3], {nameof(OrderData.OrderType)}) EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void WhereContainsArrayMember_BuildKSql_PrintsArrayContains()
  {
    //Arrange
    var orderTypes = new[] { 1, 3 };

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Where(c => K.Functions.ArrayContains(orderTypes, c.OrderType));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE ARRAY_CONTAINS(ARRAY[1, 3], {nameof(OrderData.OrderType)}) EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void WhereContainsListMember_BuildKSql_PrintsWhere()
  {
    //Arrange
    var orderTypes = new List<int> { 1, 3 };

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Where(c => orderTypes.Contains(c.OrderType));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE {nameof(OrderData.OrderType)} IN (1, 3) EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void WhereContainsListOfStringsMember_BuildKSql_PrintsWhere()
  {
    //Arrange
    var orderTypes = new List<string> { "1", "3" };

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Where(c => orderTypes.Contains(c.Category));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE {nameof(OrderData.Category)} IN ('1', '3') EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void WhereContainsArrayMember_BuildKSql_PrintsWhere()
  {
    //Arrange
    var orderTypes = new[] { 1, 3 };

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<OrderData>()
      .Where(c => orderTypes.Contains(c.OrderType));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE {nameof(OrderData.OrderType)} IN (1, 3) EMIT CHANGES;".ReplaceLineEndings());
  }

  #endregion

  #region Between

  [Test]
  public void WhereBetween_StringField_PrintsBetween()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Where(p => p.Message.Between("1", "3"));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT * FROM Tweets
WHERE {nameof(Tweet.Message)} BETWEEN '1' AND '3' EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void WhereBetween_IntegerField_PrintsBetween()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Where(p => p.Id.Between(1, 3));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT * FROM Tweets
WHERE {nameof(Tweet.Id)} BETWEEN 1 AND 3 EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectBetween_IntegerField_PrintsBetween()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Select(p => p.Id.Between(1, 3));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(Tweet.Id)} BETWEEN 1 AND 3 FROM Tweets EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectBetweenAsAlias_IntegerField_PrintsBetween()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Select(p => new { IsBetween = p.Id.Between(1, 3) });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(Tweet.Id)} BETWEEN 1 AND 3 IsBetween FROM Tweets EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectBetweenFromVariables_IntegerField_PrintsBetween()
  {
    //Arrange
    int startExpression = 1;
    int endExpression = 3;

    var query = CreateTweetsStreamSource()
      .Select(p => new { IsBetween = p.Id.Between(startExpression, endExpression) });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(Tweet.Id)} BETWEEN 1 AND 3 IsBetween FROM Tweets EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectBetweenFromConstants_IntegerField_PrintsBetween()
  {
    //Arrange
    int startExpression = 1;
    int endExpression = 3;

    var query = CreateTweetsStreamSource()
      .Select(p => new { IsBetween = 3.Between(startExpression, endExpression) });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      "SELECT 3 BETWEEN 1 AND 3 IsBetween FROM Tweets EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void WhereNotBetween_StringField_PrintsBetween()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Where(p => p.Message.NotBetween("1", "3"));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT * FROM Tweets
WHERE {nameof(Tweet.Message)} NOT BETWEEN '1' AND '3' EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  private record TimeTypes
  {
    public DateTime Dt { get; set; }
    public TimeSpan Ts { get; set; }
    public DateTimeOffset DtOffset { get; set; }
  }

  [Test]
  public void WhereBetween_TimeClosure_PrintsBetween()
  {
    //Arrange
    var from = new DateTime(2021, 10, 1);
    var to = new DateTime(2021, 10, 12);

    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Where(p => p.Dt.Between(from, to));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT * FROM TimeTypes
WHERE {nameof(CreateEntityTests.TimeTypes.Dt)} BETWEEN '2021-10-01' AND '2021-10-12' EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void WhereBetween_TimeField_PrintsBetween()
  {
    //Arrange
    var query = new TestableDbProvider(contextOptions)
      .CreatePushQuery<TimeTypes>()
      .Where(p => p.Dt.Between(new DateTime(2021, 10, 1), new DateTime(2021, 10, 12)));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT * FROM TimeTypes
WHERE {nameof(CreateEntityTests.TimeTypes.Dt)} BETWEEN '2021-10-01' AND '2021-10-12' EMIT CHANGES;".ReplaceLineEndings().ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Take

  [Test]
  public void Take_BuildKSql_PrintsLimit()
  {
    //Arrange
    int limit = 2;

    var query = CreateStreamSource()
      .Take(limit);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo($"SELECT * FROM {streamName} EMIT CHANGES LIMIT {limit};");
  }

  #endregion

  #region ToQueryString

  [Test]
  public void ToQueryString_BuildKSql_PrintsQuery()
  {
    //Arrange
    int limit = 2;

    var query = CreateStreamSource()
      .Take(limit);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    ksql.Should().BeEquivalentTo($"SELECT * FROM {streamName} EMIT CHANGES LIMIT {limit};");
  }

  #endregion

  #region StreamName

  [Test]
  public void DontPluralize_BuildKSql_PrintsSingularStreamName()
  {
    //Arrange
    var query = CreateStreamSource(shouldPluralizeStreamName: false);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT * FROM {nameof(Location)} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void InjectStreamName_BuildKSql_PrintsInjectedStreamName()
  {
    //Arrange
    queryContext.FromItemName = "Custom_Stream_Name";
    var query = CreateStreamSource();

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT * FROM {queryContext.FromItemName}s EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void InjectStreamName_ShouldNotPluralizeStreamName_BuildKSql_PrintsInjectedStreamName()
  {
    //Arrange
    var query = CreateStreamSource(shouldPluralizeStreamName: false);
    queryContext.FromItemName = "Custom_Stream_Name";

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT * FROM {queryContext.FromItemName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Arrays

  [Test]
  public void SelectArrayLength_BuildKSql_PrintsArrayLength()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new[] { 1, 2, 3 }.Length);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY_LENGTH(ARRAY[1, 2, 3]) FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectNamedArrayLength_BuildKSql_PrintsArrayLength()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { new[] { 1, 2, 3 }.Length });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY_LENGTH(ARRAY[1, 2, 3]) Length FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectArrayIndex_BuildKSql_PrintsArrayIndex()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { FirstItem = new[] { 1, 2, 3 }[1] });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[1, 2, 3][1] AS FirstItem FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void ArrayProjected()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { Str = new[] { 1, 2, 3 } });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[1, 2, 3] Str FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Maps

  [Test]
  public void SelectDictionary_BuildKSql_PrintsMap()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new Dictionary<string, int>
      {
        { "c", 2 },
        { "d", 4 }
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('c' := 2, 'd' := 4) FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectDictionaryProjected_BuildKSql_PrintsMap()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Map = new Dictionary<string, int>
        {
          { "c", 2 },
          { "d", 4 }
        }
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('c' := 2, 'd' := 4) Map FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectDictionaryElement_BuildKSql_PrintsMapElementAccess()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new Dictionary<string, int>
      {
        { "c", 2 },
        { "d", 4 }
      }["d"]);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('c' := 2, 'd' := 4)['d'] FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectDictionaryElementProjected_BuildKSql_PrintsMapElementAccess()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Element = new Dictionary<string, int>
        {
          { "c", 2 },
          { "d", 4 }
        }["d"]
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('c' := 2, 'd' := 4)['d'] Element FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Structs

  private struct Point
  {
    public int X { get; init; }

    public int Y { get; set; }
  }

  [Test]
  public void SelectStruct_BuildKSql_PrintsStruct()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new Point { X = 1, Y = 2 });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT STRUCT(X := 1, Y := 2) FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectStructProjected_BuildKSql_PrintsStruct()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { V = new Point { X = 1, Y = 2 } });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT STRUCT(X := 1, Y := 2) V FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectStructElement_BuildKSql_PrintsElementAccessor()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new Point { X = 1, Y = 2 }.X);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT STRUCT(X := 1, Y := 2)->X FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectStructElementProjected_BuildKSql_PrintsElementAccessor()
  {
    //Arrange
#pragma warning disable IDE0037
    var query = CreateStreamSource()
      .Select(c => new { X = new Point { X = 1, Y = 2 }.X });
#pragma warning restore IDE0037

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT STRUCT(X := 1, Y := 2)->X X FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  private struct LocationStruct
  {
    public string X { get; set; }
    public double Y { get; set; }
    public string[] Arr { get; set; }
    public Dictionary<string, double> Map { get; set; }
  }

  [Test]
  public void SelectStructElementsFromColumns_BuildKSql_PrintsStruct()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new { LS = new LocationStruct { X = c.Latitude, Y = c.Longitude }, Text = "text" });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT STRUCT(X := {nameof(Location.Latitude)}, Y := {nameof(Location.Longitude)}) LS, 'Text' Text FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectStructWithNestedArray_BuildKSql_PrintsStruct()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        LS = new LocationStruct
        {
          X = c.Latitude,
          Arr = new[] { c.Latitude, c.Latitude },
          Y = c.Longitude,
        },
        Text = "text"
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT STRUCT(X := {nameof(Location.Latitude)}, Arr := ARRAY[{nameof(Location.Latitude)}, {nameof(Location.Latitude)}], Y := {nameof(Location.Longitude)}) LS, 'Text' Text FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectStructWithNestedMap_BuildKSql_PrintsStruct()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        LS = new LocationStruct
        {
          X = c.Latitude,
          Map = new Dictionary<string, double>
          {
            { "c", c.Longitude },
            { "d", 4 }
          },
          Y = c.Longitude,
        },
        Text = "text"
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT STRUCT(X := {nameof(Location.Latitude)}, Map := MAP('c' := {nameof(Location.Longitude)}, 'd' := 4), Y := {nameof(Location.Longitude)}) LS, 'Text' Text FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  private record DatabaseChangeObject<TEntity>
  {
    public TEntity After { get; init; } = default!;
  }

  private record Entity
  {
    public string SensorId { get; init; } = null!;
    public Model Model { get; init; } = null!;
  }

  private record Model
  {
    public string Version { get; init; } = null!;
  }

  private IQbservable<DatabaseChangeObject<Entity>> CreateDatabaseChangeObjectStreamSource()
  {
    var context = new TestableDbProvider(contextOptions);

    return context.CreatePushQuery<DatabaseChangeObject<Entity>>();
  }

  [Test]
  public void SelectNestedProperty_BuildKSql_PrintsElementAccessor()
  {
    //Arrange
    var query = CreateDatabaseChangeObjectStreamSource()
      .Select(c => c.After.SensorId);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(DatabaseChangeObject<object>.After)}->{nameof(Entity.SensorId)} FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectDeeplyNestedProperty_BuildKSql_PrintsElementAccessor()
  {
    //Arrange
    var query = CreateDatabaseChangeObjectStreamSource()
      .Select(c => c.After.Model.Version);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(DatabaseChangeObject<object>.After)}->{nameof(Entity.Model)}->{nameof(Model.Version)} FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectNewNestedProperty_BuildKSql_PrintsElementAccessor()
  {
    //Arrange
    var query = CreateDatabaseChangeObjectStreamSource()
      .Select(c => new { c.After.SensorId });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT {nameof(DatabaseChangeObject<object>.After)}->{nameof(Entity.SensorId)} FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void SelectNewNestedPropertyWithAlias_BuildKSql_PrintsElementAccessor()
  {
    //Arrange
    var query = CreateDatabaseChangeObjectStreamSource()
      .Select(c => new { X = c.After.SensorId, Y = c.After.SensorId.Length, Substr = K.Functions.Substring(c.After.SensorId, 2) });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string after = nameof(DatabaseChangeObject<object>.After);
    string substr = $"SUBSTRING({after}->{nameof(Entity.SensorId)}, 2)";
    string expectedKsql =
      $"SELECT {after}->{nameof(Entity.SensorId)} AS X, LEN({after}->{nameof(Entity.SensorId)}) AS Y, {substr} Substr FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Deeply nested types

  [Test]
  public void NestedArrayInMap()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Map = new Dictionary<string, int[]>
        {
          { "a", new[] { 1, 2 } },
          { "b", new[] { 3, 4 } },
        }
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4]) Map FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedMapInMap()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Map = new Dictionary<string, Dictionary<string, int>>
        {
          { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
          { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
        }
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4)) Map FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedStructInMap()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Str = new Dictionary<string, LocationStruct>
        {
          { "a", new LocationStruct
          {
            X = c.Latitude,
            Y = c.Longitude,
          } },
          { "b", new LocationStruct
          {
            X = "test",
            Y = 1,
          } },
        }
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('a' := STRUCT(X := {nameof(Location.Latitude)}, Y := {nameof(Location.Longitude)}), 'b' := STRUCT(X := 'test', Y := 1)) Str FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedMapInArray()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Arr = new[]
        {
          new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
          new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
        }
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)] Arr FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedArrayInArray()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Arr = new[]
        {
          new [] { 1, 2 },
          new [] { 3, 4 },
        }
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[ARRAY[1, 2], ARRAY[3, 4]] Arr FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedStructInArray()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Arr = new[]
        {
          new LocationStruct
          {
            X = c.Latitude,
            Y = c.Longitude,
          }, new LocationStruct
          {
            X = "test",
            Y = 1,
          }
        },
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[STRUCT(X := {nameof(Location.Latitude)}, Y := {nameof(Location.Longitude)}), STRUCT(X := 'test', Y := 1)] Arr FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedArrayInArray_OuterMemberAccess()
  {
    //Arrange
    var nestedArrays = new[]
    {
      new[] {1, 2},
      [3, 4],
    };

    var query = CreateStreamSource()
      .Select(c => new
      {
        Arr = nestedArrays
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[ARRAY[1, 2], ARRAY[3, 4]] AS Arr FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Deeply nested types element destructure

  [Test]
  public void NestedArrayInMap_ElementAccess()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Map = new Dictionary<string, int[]>
        {
          { "a", new[] { 1, 2 } },
          { "b", new[] { 3, 4 } },
        }["a"][1]
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4])['a'][1] AS Map FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedMapInMap_ElementAccess()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Map = new Dictionary<string, Dictionary<string, int>>
        {
          { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
          { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
        }["a"]["d"]
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4))['a']['d'] Map FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedMapInArray_ElementAccess()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Arr = new[]
        {
          new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
          new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
        }[1]["d"]
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)][1]['d'] Arr FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void NestedArrayInArray_ElementAccess()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c => new
      {
        Arr = new[]
        {
          new [] { 1, 2},
          new [] { 3, 4},
        }[0][1]
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ARRAY[ARRAY[1, 2], ARRAY[3, 4]][0][1] AS Arr FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Operators

  [Test]
  public void LogicalOperatorNot_BuildKSql_PrintsNot()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Select(l => !l.IsRobot);

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT NOT {nameof(Tweet.IsRobot)} FROM {nameof(Tweet)}s EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void LogicalOperatorNotProjected_BuildKSql_PrintsNot()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Select(l => new { NotRobot = !l.IsRobot });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT NOT {nameof(Tweet.IsRobot)} NotRobot FROM {nameof(Tweet)}s EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void Contains_BuildKSql_PrintsLike()
  {
    //Arrange
    var query = CreateTweetsStreamSource()
      .Where(c => c.Message.ToLower().Contains("hard".ToLower()));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT * FROM {nameof(Tweet)}s
WHERE LCASE(Message) LIKE LCASE('%hard%') EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  [Test]
  public void EndsWith_BuildKSql_PrintsLike()
  {
    //Arrange
    string movie = "hard";

    var query = CreateTweetsStreamSource()
      .Where(c => c.Message.EndsWith(movie.ToUpper()));

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      @$"SELECT * FROM {nameof(Tweet)}s
WHERE Message LIKE UCASE('%hard') EMIT CHANGES;".ReplaceLineEndings();

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Functions

  [Test]
  public void EntriesFromDictionary_BuildKSql_PrintsFunction()
  {
    //Arrange
    bool sorted = true;
    var query = CreateStreamSource()
      .Select(c => new
      {
        Col = KSqlFunctions.Instance.Entries(new Dictionary<string, string>()
        {
          { "a", "value" }
        }, sorted)
      });

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT ENTRIES(MAP('a' := 'value'), true) Col FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  #endregion

  #region Case

  [Test]
  public void Switch_BuildKSql_PrintsCaseWhen()
  {
    //Arrange
    var query = CreateStreamSource()
      .Select(c =>
        new
        {
          case_result =
            (c.Longitude < 2.0) ? "small" :
            (c.Longitude < 4.1) ? "medium" : "large"
        }
      );

    //Act
    var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

    //Assert
    string expectedKsql =
      $"SELECT CASE WHEN {nameof(Location.Longitude)} < 2 THEN 'small' WHEN {nameof(Location.Longitude)} < 4.1 THEN 'medium' ELSE 'large' END AS case_result FROM {streamName} EMIT CHANGES;";

    ksql.Should().BeEquivalentTo(expectedKsql);
  }

  //TODO:SwitchExpressions
  private static string SwitchExpressionProvider()
  {
    var location = new Location();

    var caseResult = location.Longitude switch
    {
      < 2.0 => "small",
      <= 4.0 => "medium",
      _ => "large"
    };

    return caseResult;
  }

  //TODO:IfElse
  private static string IfElseProvider(double value)
  {
    if (value < 2.0)
    { return "small"; }
    if (value <= 4.0)
    { return "medium"; }
    else
    { return "large"; }
  }

  #endregion
}

#pragma warning restore CA1861
