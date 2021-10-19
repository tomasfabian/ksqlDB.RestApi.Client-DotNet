using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Query.Functions
{
  [TestClass]
  public class KSqlInvocationFunctionsTests : Infrastructure.IntegrationTests
  {
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      RestApiProvider = KSqlDbRestApiProvider.Create();
      
      var statement =
        new KSqlDbStatement(
          @"CREATE STREAM stream2 (id INT, lambda_arr ARRAY<INTEGER>) WITH (kafka_topic = 'stream2', partitions = 1, value_format = 'json');");
      
      var response = await RestApiProvider.ExecuteStatementAsync(statement);

      var statement2 =
        new KSqlDbStatement(
          @"CREATE OR REPLACE STREAM stream4 (id INT, lambda_map MAP<STRING,ARRAY<INTEGER>>) WITH (kafka_topic = 'stream4', partitions = 1, value_format = 'json');");

      response = await RestApiProvider.ExecuteStatementAsync(statement2);

      string insertIntoStream3 = "insert into stream4 (id, lambda_map) values (1, MAP('hello':= ARRAY [1,2,3], 'goodbye':= ARRAY [-1,-2,-3]) );";

      response = await RestApiProvider.ExecuteStatementAsync(
        new KSqlDbStatement("insert into stream2 (id, lambda_arr) values (1, ARRAY [1,2,3]);"));
      response = await RestApiProvider.ExecuteStatementAsync(
        new KSqlDbStatement(insertIntoStream3));
    }

    record Lambda
    {
      public int Id { get; set; }
      public int[] Lambda_Arr { get; set; }
      // public IEnumerable<int> Lambda_Arr { get; set; }
    }

    private readonly string streamName = "stream2";

    [TestMethod]
    public async Task TransformArray()
    {
      //Arrange
      int expectedItemsCount = 1;

      //Act
      var source = Context.CreateQuery<Lambda>(streamName)
        .Select(c => new { Col = KSqlFunctions.Instance.Transform(c.Lambda_Arr, x => x + 1) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Should().BeEquivalentTo(new[] {2,3,4});
    }

    class LambdaMap
    {
      public int Id { get; set; }
      public IDictionary<string, int[]> Lambda_Map { get; set; }
      public IDictionary<string, int> Dictionary2 { get; set; }
    }
    
    private readonly string streamNameWithMap = "stream4";

    [TestMethod]
    public async Task TransformMap()
    {
      //Arrange
      int expectedItemsCount = 1;      
      
      //Act
      var source = Context.CreateQuery<LambdaMap>(streamNameWithMap)
        .Select(c => new { Col = K.Functions.Transform(c.Lambda_Map, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v, x => x * x)) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Keys.First().Should().Be("goodbye_new");
      actualValues[0].Col.Values.Count.Should().Be(2);
      actualValues[0].Col.Values.First().Should().BeEquivalentTo(new [] {1,4,9});
    }

    [TestMethod]
    public async Task FilterArray()
    {
      //Arrange
      int expectedItemsCount = 1;

      //Act
      var source = Context.CreateQuery<Lambda>(streamName)
        .Select(c => new { Col = KSqlFunctions.Instance.Filter(c.Lambda_Arr, x => x > 1) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Should().BeEquivalentTo(new[] { 2, 3 });
    }
    
    [TestMethod]
    public async Task FilterMap()
    {
      //Arrange
      int expectedItemsCount = 1;      
      
      //Act
      var source = Context.CreateQuery<LambdaMap>(streamNameWithMap)
        .Select(c => new { Col = K.Functions.Filter(c.Lambda_Map, (k, v) => k != "E.T" && v[1] > 0) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Values.Count.Should().Be(1);
      actualValues[0].Col.Values.First().Should().BeEquivalentTo(new [] {1,2,3});
    }

    [TestMethod]
    public async Task ReduceArray()
    {
      //Arrange
      int expectedItemsCount = 1;

      //Act
      var source = Context.CreateQuery<Lambda>(streamName)
        .Select(c => new { Acc = K.Functions.Reduce(c.Lambda_Arr, 0, (x,y) => x + y) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Acc.Should().Be(6);
    }
    
    [TestMethod]
    public async Task ReduceMap()
    {
      //Arrange
      int expectedItemsCount = 1;      
      
      //Act
      var source = Context.CreateQuery<LambdaMap>(streamNameWithMap)
        .Select(c => new { Col = K.Functions.Reduce(c.Lambda_Map, 2, (s, k, v) => K.Functions.Ceil(s / v[1])) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Should().Be(-2);
    }
  }
}