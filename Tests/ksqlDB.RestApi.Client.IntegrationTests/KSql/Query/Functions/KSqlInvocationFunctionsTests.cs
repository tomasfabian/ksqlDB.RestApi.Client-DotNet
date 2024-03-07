using System.Net;
using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Query.Functions;

public class KSqlInvocationFunctionsTests : Infrastructure.IntegrationTests
{
  [OneTimeSetUp]
  public static async Task ClassInitialize()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();

    var statement =
      new KSqlDbStatement(
        $"CREATE STREAM {StreamName} (id INT, arr ARRAY<INTEGER>) WITH (kafka_topic = '{StreamName}', partitions = 1, value_format = 'json');");
      
    var response = await RestApiProvider.ExecuteStatementAsync(statement);
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var statement2 =
      new KSqlDbStatement(
        $"CREATE OR REPLACE STREAM {StreamName4} (id INT, map MAP<STRING,ARRAY<INTEGER>>) WITH (kafka_topic = '{StreamName4}', partitions = 1, value_format = 'json');");

    response = await RestApiProvider.ExecuteStatementAsync(statement2);
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    string insertIntoStream3 = $"insert into {StreamName4} (id, map) values (1, MAP('hello':= ARRAY [1,2,3], 'goodbye':= ARRAY [-1,-2,-3]) );";

    response = await RestApiProvider.ExecuteStatementAsync(
      new KSqlDbStatement($"insert into {StreamName} (id, arr) values (1, ARRAY [1,2,3]);"));
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response = await RestApiProvider.ExecuteStatementAsync(
      new KSqlDbStatement(insertIntoStream3));
    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [OneTimeTearDown]
  public static async Task ClassCleanup()
  {
    await RestApiProvider.DropStreamAndTopic(StreamName);
    await RestApiProvider.DropStreamAndTopic(StreamName4);
  }

  private record Lambda
  {
    public int Id { get; set; }
    public int[] Arr { get; set; } = null!;
  }

  private const string StreamName = "stream2";
  private const string StreamName4 = "stream4";

  [Test]
  public async Task TransformArray()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreateQuery<Lambda>(StreamName)
      .Select(c => new { Col = KSqlFunctions.Instance.Transform(c.Arr, x => x + 1) })
      .ToAsyncEnumerable();
      
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Col.Should().BeEquivalentTo(new[] {2,3,4});
  }

  private class LambdaMap
  {
    public int Id { get; set; }
    public IDictionary<string, int[]> Map { get; set; } = null!;
    public IDictionary<string, int> Dictionary2 { get; set; } = null!;
  }
    
  private readonly string streamNameWithMap = "stream4";

  [Test]
  public async Task TransformMap()
  {
    //Arrange
    int expectedItemsCount = 1;      
      
    //Act
    var source = Context.CreateQuery<LambdaMap>(streamNameWithMap)
      .Select(c => new { Col = K.Functions.Transform(c.Map, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v, x => x * x)) })
      .ToAsyncEnumerable();
      
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Col.Keys.First().Should().Be("goodbye_new");
    actualValues[0].Col.Values.Count.Should().Be(2);
    actualValues[0].Col.Values.First().Should().BeEquivalentTo(new [] {1,4,9});
  }

  [Test]
  public async Task FilterArray()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreateQuery<Lambda>(StreamName)
      .Select(c => new { Col = KSqlFunctions.Instance.Filter(c.Arr, x => x > 1) })
      .ToAsyncEnumerable();
      
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Col.Should().BeEquivalentTo(new[] { 2, 3 });
  }
    
  [Test]
  public async Task FilterMap()
  {
    //Arrange
    int expectedItemsCount = 1;      
      
    //Act
    var source = Context.CreateQuery<LambdaMap>(streamNameWithMap)
      .Select(c => new { Col = K.Functions.Filter(c.Map, (k, v) => k != "E.T" && v[1] > 0) })
      .ToAsyncEnumerable();
      
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Col.Values.Count.Should().Be(1);
    actualValues[0].Col.Values.First().Should().BeEquivalentTo(new [] {1,2,3});
  }

  [Test]
  public async Task ReduceArray()
  {
    //Arrange
    int expectedItemsCount = 1;

    //Act
    var source = Context.CreateQuery<Lambda>(StreamName)
      .Select(c => new { Acc = K.Functions.Reduce(c.Arr, 0, (x,y) => x + y) })
      .ToAsyncEnumerable();
      
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Acc.Should().Be(6);
  }
    
  [Test]
  public async Task ReduceMap()
  {
    //Arrange
    int expectedItemsCount = 1;      
      
    //Act
    var source = Context.CreateQuery<LambdaMap>(streamNameWithMap)
      .Select(c => new { Col = K.Functions.Reduce(c.Map, 2, (s, k, v) => K.Functions.Ceil(s / v[1])) })
      .ToAsyncEnumerable();
      
    var actualValues = await CollectActualValues(source, expectedItemsCount);
      
    //Assert
    Assert.AreEqual(expectedItemsCount, actualValues.Count);
    actualValues[0].Col.Should().Be(-2);
  }
}
