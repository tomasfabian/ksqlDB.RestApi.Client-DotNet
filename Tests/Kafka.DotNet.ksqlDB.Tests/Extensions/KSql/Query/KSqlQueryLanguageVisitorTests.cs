using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Kafka.DotNet.ksqlDB.KSql.Query.Operators;
using Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Linq;
using Kafka.DotNet.ksqlDB.Tests.Helpers;
using Kafka.DotNet.ksqlDB.Tests.Pocos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;
using Location = Kafka.DotNet.ksqlDB.Tests.Models.Location;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query
{
  [TestClass]
  public class KSqlQueryLanguageVisitorTests : TestBase
  {
    private KSqlQueryGenerator ClassUnderTest { get; set; }

    string streamName = nameof(Location) + "s";

    private KSqlDBContextOptions contextOptions;
    private QueryContext queryContext;

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      contextOptions = new KSqlDBContextOptions(TestParameters.KsqlDBUrl);
      queryContext = new QueryContext();
      ClassUnderTest = new KSqlQueryGenerator(contextOptions);
    }

    private IQbservable<Location> CreateStreamSource(bool shouldPluralizeStreamName = true)
    {
      contextOptions.ShouldPluralizeFromItemName = shouldPluralizeStreamName;

      var context = new TestableDbProvider(contextOptions);

      return context.CreateQueryStream<Location>();
    }

    private IQbservable<Tweet> CreateTweetsStreamSource()
    {
      var context = new TestableDbProvider(contextOptions);

      return context.CreateQueryStream<Tweet>();
    }

    #region Select

    [TestMethod]
    public void SelectAlias_BuildKSql_PrintsProjection()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(l => new { Lngt = l.Longitude, l.Latitude });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(Location.Longitude)} AS Lngt, {nameof(Location.Latitude)} FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectTwoAliasesWithBinaryOperations_BuildKSql_PrintsProjection()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(l => new { Lngt = l.Longitude / 2, Lat = l.Latitude + "" });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(Location.Longitude)} / 2 AS Lngt, {nameof(Location.Latitude)} + '' AS Lat FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
WHERE {nameof(Location.Latitude)} = '1' EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectMultipleWhere_BuildKSql_PrintsSelectFromWheres()
    {
      //Arrange
      var query = CreateStreamSource()
        .Where(p => p.Latitude == "1")
        .Where(p => p.Longitude == 0.1)
        .Select(l => new { l.Longitude, l.Latitude });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(Location.Longitude)}, {nameof(Location.Latitude)} FROM {streamName}
WHERE {nameof(Location.Latitude)} = '1' AND {nameof(Location.Longitude)} = 0.1 EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectDynamicFunction_BuildKSql_PrintsFunctionCall()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new { c.Longitude, c.Latitude, Col = ksqlDB.KSql.Query.Functions.KSql.F.Dynamic("IFNULL(Latitude, 'n/a')") as string });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(Location.Longitude)}, {nameof(Location.Latitude)}, IFNULL(Latitude, 'n/a') Col FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectListMember_BuildKSql_PrintsArray()
    {
      //Arrange
      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Select(l => new List<int> { 1, 3 });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql = @$"SELECT ARRAY[1, 3] FROM {nameof(OrderData)} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectList_BuildKSql_PrintsArray()
    {
      //Arrange
      var orderTypes = new List<int> { 1, 3 };

      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Select(l => new { OrderTypes = orderTypes });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql = @$"SELECT ARRAY[1, 3] AS OrderTypes FROM {nameof(OrderData)} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectListContains_BuildKSql_PrintsIn()
    {
      //Arrange
      var orderTypes = new List<int> { 1, 3 };

      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Select(c => orderTypes.Contains(c.OrderType));

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT {nameof(OrderData.OrderType)} IN (1, 3) FROM {nameof(OrderData)} EMIT CHANGES;");
    }

    [TestMethod]
    public void SelectNewListContains_BuildKSql_PrintsIn()
    {
      //Arrange
      var orderTypes = new List<int> { 1, 3 };

      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Select(c => new { Contains = new List<int> { 1, 3 }.Contains(c.OrderType) });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT {nameof(OrderData.OrderType)} IN (1, 3) Contains FROM {nameof(OrderData)} EMIT CHANGES;");
    }

    #region CapturedVariables

    [TestMethod]
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
      ksql.Should().Be($"SELECT TRANSFORM(MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('a' := 3, 'd' := 4)), (k, v) => UCASE(k), (k, v) => v['a'] + 1) Dict FROM {streamName} EMIT CHANGES;");
    }

    struct Foo
    {
      public int Prop { get; set; }
    }

    [TestMethod]
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
      ksql.Should().Be($"SELECT STRUCT(Prop := 42) AS C FROM {streamName} EMIT CHANGES;");
    }

    [TestMethod]
    [Ignore("TODO:capured")]
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
      ksql.Should().Be($"SELECT STRUCT(Prop := 42) AS C FROM {streamName} EMIT CHANGES;");
    }

    #endregion

    #endregion

    #region Where

    [TestMethod]
    public void Where_BuildKSql_PrintsWhere()
    {
      //Arrange
      var query = CreateStreamSource()
        .Where(p => p.Latitude == "1");

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {streamName}
WHERE {nameof(Location.Latitude)} = '1' EMIT CHANGES;");
    }

    [TestMethod]
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
WHERE {nameof(Location.Latitude)} = '1' EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    class OrderData
    {
      public int OrderType { get; set; }
      public string Category { get; set; }
    }

    [TestMethod]
    public void WhereContains_BuildKSql_PrintsArrayContains()
    {
      //Arrange
      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Where(c => K.Functions.ArrayContains(new[] { 1, 3 }, c.OrderType));

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE ARRAY_CONTAINS(ARRAY[1, 3], {nameof(OrderData.OrderType)}) EMIT CHANGES;");
    }

    [TestMethod]
    public void WhereContainsArrayMember_BuildKSql_PrintsArrayContains()
    {
      //Arrange
      var orderTypes = new[] { 1, 3 };

      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Where(c => K.Functions.ArrayContains(orderTypes, c.OrderType));

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE ARRAY_CONTAINS(ARRAY[1, 3], {nameof(OrderData.OrderType)}) EMIT CHANGES;");
    }

    [TestMethod]
    public void WhereContainsListMember_BuildKSql_PrintsWhere()
    {
      //Arrange
      var orderTypes = new List<int> { 1, 3 };

      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Where(c => orderTypes.Contains(c.OrderType));

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE {nameof(OrderData.OrderType)} IN (1, 3) EMIT CHANGES;");
    }

    [TestMethod]
    public void WhereContainsListOfStringsMember_BuildKSql_PrintsWhere()
    {
      //Arrange
      var orderTypes = new List<string> { "1", "3" };

      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Where(c => orderTypes.Contains(c.Category));

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE {nameof(OrderData.Category)} IN ('1', '3') EMIT CHANGES;");
    }

    [TestMethod]
    public void WhereContainsArrayMember_BuildKSql_PrintsWhere()
    {
      //Arrange
      var orderTypes = new[] { 1, 3 };

      var query = new TestableDbProvider(contextOptions)
        .CreateQueryStream<OrderData>()
        .Where(c => orderTypes.Contains(c.OrderType));

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {nameof(OrderData)}
WHERE {nameof(OrderData.OrderType)} IN (1, 3) EMIT CHANGES;");
    }

    #endregion

    #region Between

    [TestMethod]
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
WHERE {nameof(Tweet.Message)} BETWEEN '1' AND '3' EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
WHERE {nameof(Tweet.Id)} BETWEEN 1 AND 3 EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectBetween_IntegerField_PrintsBetween()
    {
      //Arrange
      var query = CreateTweetsStreamSource()
        .Select(p => p.Id.Between(1, 3));

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(Tweet.Id)} BETWEEN 1 AND 3 FROM Tweets EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectBetweenAsAlias_IntegerField_PrintsBetween()
    {
      //Arrange
      var query = CreateTweetsStreamSource()
        .Select(p => new { IsBetween = p.Id.Between(1, 3) } );

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(Tweet.Id)} BETWEEN 1 AND 3 IsBetween FROM Tweets EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectBetweenFromVariables_IntegerField_PrintsBetween()
    {
      //Arrange
      int startExpression = 1;
      int endExpression = 3;

      var query = CreateTweetsStreamSource()
        .Select(p => new { IsBetween = p.Id.Between(startExpression, endExpression) } );

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(Tweet.Id)} BETWEEN 1 AND 3 IsBetween FROM Tweets EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectBetweenFromConstants_IntegerField_PrintsBetween()
    {
      //Arrange
      int startExpression = 1;
      int endExpression = 3;

      var query = CreateTweetsStreamSource()
        .Select(p => new { IsBetween = 3.Between(startExpression, endExpression) } );

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT 3 BETWEEN 1 AND 3 IsBetween FROM Tweets EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
WHERE {nameof(Tweet.Message)} NOT BETWEEN '1' AND '3' EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Take

    [TestMethod]
    public void Take_BuildKSql_PrintsLimit()
    {
      //Arrange
      int limit = 2;

      var query = CreateStreamSource()
        .Take(limit);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {streamName} EMIT CHANGES LIMIT {limit};");
    }

    #endregion

    #region ToQueryString

    [TestMethod]
    public void ToQueryString_BuildKSql_PrintsQuery()
    {
      //Arrange
      int limit = 2;

      var query = CreateStreamSource()
        .Take(limit);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      ksql.Should().BeEquivalentTo(@$"SELECT * FROM {streamName} EMIT CHANGES LIMIT {limit};");
    }

    #endregion

    #region StreamName

    [TestMethod]
    public void DontPluralize_BuildKSql_PrintsSingularStreamName()
    {
      //Arrange
      var query = CreateStreamSource(shouldPluralizeStreamName: false);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT * FROM {nameof(Location)} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void InjectStreamName_BuildKSql_PrintsInjectedStreamName()
    {
      //Arrange
      queryContext.FromItemName = "Custom_Stream_Name";
      var query = CreateStreamSource();

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT * FROM {queryContext.FromItemName}s EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void InjectStreamName_ShouldNotPluralizeStreamName_BuildKSql_PrintsInjectedStreamName()
    {
      //Arrange
      var query = CreateStreamSource(shouldPluralizeStreamName: false);
      queryContext.FromItemName = "Custom_Stream_Name";

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT * FROM {queryContext.FromItemName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Arrays

    [TestMethod]
    public void SelectArrayLength_BuildKSql_PrintsArrayLength()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new[] { 1, 2, 3 }.Length);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT ARRAY_LENGTH(ARRAY[1, 2, 3]) FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectNamedArrayLength_BuildKSql_PrintsArrayLength()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new { new[] { 1, 2, 3 }.Length });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT ARRAY_LENGTH(ARRAY[1, 2, 3]) Length FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectArrayIndex_BuildKSql_PrintsArrayIndex()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new { FirstItem = new[] { 1, 2, 3 }[1] });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT ARRAY[1, 2, 3][1] AS FirstItem FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void ArrayProjected()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new { Str = new[] { 1, 2, 3 } });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT ARRAY[1, 2, 3] Str FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Maps

    [TestMethod]
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
        @$"SELECT MAP('c' := 2, 'd' := 4) FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT MAP('c' := 2, 'd' := 4) Map FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT MAP('c' := 2, 'd' := 4)['d'] FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT MAP('c' := 2, 'd' := 4)['d'] Element FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Structs

    private struct Point
    {
      public int X { get; set; }

      public int Y { get; set; }
    }

    [TestMethod]
    public void SelectStruct_BuildKSql_PrintsStruct()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new Point { X = 1, Y = 2 });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT STRUCT(X := 1, Y := 2) FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectStructProjected_BuildKSql_PrintsStruct()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new { V = new Point { X = 1, Y = 2 } });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT STRUCT(X := 1, Y := 2) V FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectStructElement_BuildKSql_PrintsElementAccessor()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new Point { X = 1, Y = 2 }.X);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT STRUCT(X := 1, Y := 2)->X FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectStructElementProjected_BuildKSql_PrintsElementAccessor()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new { X = new Point { X = 1, Y = 2 }.X });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT STRUCT(X := 1, Y := 2)->X X FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    private struct LocationStruct
    {
      public string X { get; set; }
      public double Y { get; set; }
      public string[] Arr { get; set; }
      public Dictionary<string, double> Map { get; set; }
    }

    [TestMethod]
    public void SelectStructElementsFromColumns_BuildKSql_PrintsStruct()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new { LS = new LocationStruct { X = c.Latitude, Y = c.Longitude }, Text = "text" });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT STRUCT(X := {nameof(Location.Latitude)}, Y := {nameof(Location.Longitude)}) LS, 'Text' Text FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT STRUCT(X := {nameof(Location.Latitude)}, Arr := ARRAY[{nameof(Location.Latitude)}, {nameof(Location.Latitude)}], Y := {nameof(Location.Longitude)}) LS, 'Text' Text FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT STRUCT(X := {nameof(Location.Latitude)}, Map := MAP('c' := {nameof(Location.Longitude)}, 'd' := 4), Y := {nameof(Location.Longitude)}) LS, 'Text' Text FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }
    
    record DatabaseChangeObject<TEntity>
    {
      public TEntity After { get; set; }
    }

    record Entity
    {
      public string SensorId { get; set; }
      public Model Model { get; set; }
    }

    record Model
    {
      public string Version { get; set; }
    }
    
    private IQbservable<DatabaseChangeObject<Entity>> CreateDatabaseChangeObjectStreamSource()
    {
      var context = new TestableDbProvider(contextOptions);

      return context.CreateQueryStream<DatabaseChangeObject<Entity>>();
    }

    [TestMethod]
    public void SelectNestedProperty_BuildKSql_PrintsElementAccessor()
    {
      //Arrange
      var query = CreateDatabaseChangeObjectStreamSource()
        .Select(c => c.After.SensorId);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(DatabaseChangeObject<object>.After)}->{nameof(Entity.SensorId)} FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectDeeplyNestedProperty_BuildKSql_PrintsElementAccessor()
    {
      //Arrange
      var query = CreateDatabaseChangeObjectStreamSource()
        .Select(c => c.After.Model.Version);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(DatabaseChangeObject<object>.After)}->{nameof(Entity.Model)}->{nameof(Model.Version)} FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void SelectNewNestedProperty_BuildKSql_PrintsElementAccessor()
    {
      //Arrange
      var query = CreateDatabaseChangeObjectStreamSource()
        .Select(c => new { c.After.SensorId });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT {nameof(DatabaseChangeObject<object>.After)}->{nameof(Entity.SensorId)} FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT {after}->{nameof(Entity.SensorId)} AS X, LEN({after}->{nameof(Entity.SensorId)}) AS Y, {substr} Substr FROM {nameof(DatabaseChangeObject<object>)}s EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Deeply nested types

    [TestMethod]
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
        @$"SELECT MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4]) Map FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4)) Map FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT MAP('a' := STRUCT(X := {nameof(Location.Latitude)}, Y := {nameof(Location.Longitude)}), 'b' := STRUCT(X := 'test', Y := 1)) Str FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)] Arr FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void NestedArrayInArray()
    {
      //Arrange
      var query = CreateStreamSource()
        .Select(c => new
        {
          Arr = new[]
          {
            new [] { 1, 2},
            new [] { 3, 4},
          }
        });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT ARRAY[ARRAY[1, 2], ARRAY[3, 4]] Arr FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }
    
    [TestMethod]
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
        @$"SELECT ARRAY[STRUCT(X := {nameof(Location.Latitude)}, Y := {nameof(Location.Longitude)}), STRUCT(X := 'test', Y := 1)] Arr FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    [Ignore("TODO")]
    public void NestedArrayInArray_OuterMemberAccess()
    {
      //Arrange
      var nestedArrays = new[]
      {
        new[] {1, 2},
        new[] {3, 4},
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
        @$"SELECT ARRAY[ARRAY[1, 2], ARRAY[3, 4]] Arr FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Deeply nested types element destructure

    [TestMethod]
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
        @$"SELECT MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4])['a'][1] AS Map FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4))['a']['d'] Map FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)][1]['d'] Arr FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
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
        @$"SELECT ARRAY[ARRAY[1, 2], ARRAY[3, 4]][0][1] AS Arr FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Operators

    [TestMethod]
    public void LogicalOperatorNot_BuildKSql_PrintsNot()
    {
      //Arrange
      var query = CreateTweetsStreamSource()
        .Select(l => !l.IsRobot);

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT NOT {nameof(Tweet.IsRobot)} FROM {nameof(Tweet)}s EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    [TestMethod]
    public void LogicalOperatorNotProjected_BuildKSql_PrintsNot()
    {
      //Arrange
      var query = CreateTweetsStreamSource()
        .Select(l => new { NotRobot = !l.IsRobot });

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT NOT {nameof(Tweet.IsRobot)} NotRobot FROM {nameof(Tweet)}s EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Functions

    [TestMethod]
    public void EntriesFromDictionary_BuildKSql_PrintsFunction()
    {
      //Arrange
      bool sorted = true;
      var query = CreateStreamSource()
        .Select(c => new { Col = KSqlFunctions.Instance.Entries(new Dictionary<string, string>()
          {
            { "a", "value" }
          }, sorted)});

      //Act
      var ksql = ClassUnderTest.BuildKSql(query.Expression, queryContext);

      //Assert
      string expectedKsql =
        @$"SELECT ENTRIES(MAP('a' := 'value'), true) Col FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    #endregion

    #region Case

    [TestMethod]
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
        @$"SELECT CASE WHEN {nameof(Location.Longitude)} < 2 THEN 'small' WHEN {nameof(Location.Longitude)} < 4.1 THEN 'medium' ELSE 'large' END AS case_result FROM {streamName} EMIT CHANGES;";

      ksql.Should().BeEquivalentTo(expectedKsql);
    }

    //TODO:SwitchExpressions
    private static string SwitchExpressionProvider()
    {
      var location = new Location();

      var case_result = location.Longitude switch
      {
        var value when value < 2.0  => "small",
        var value when (value <= 4.0) => "medium", 
        _ => "large"
      };

      return case_result;
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
}