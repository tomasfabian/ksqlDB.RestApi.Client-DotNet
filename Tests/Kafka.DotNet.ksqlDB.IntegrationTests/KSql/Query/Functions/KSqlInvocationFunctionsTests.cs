using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Query.Functions
{
  [TestClass]
  public class KSqlInvocationFunctionsTests : Linq.IntegrationTests
  {
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      RestApiProvider = KSqlDbRestApiProvider.Create();
      
      var statement =
        new KSqlDbStatement(
          @"CREATE STREAM stream2 (id INT, lambda_arr ARRAY<INTEGER>) WITH (kafka_topic = 'stream2', partitions = 1, value_format = 'json');");

      var response = await RestApiProvider.ExecuteStatementAsync(statement);

      response = await RestApiProvider.ExecuteStatementAsync(
        new KSqlDbStatement("insert into stream2 (id, lambda_arr) values (1, ARRAY [1,2,3]);"));
      //await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
    }

    record Lambda
    {
      public int Id { get; set; }
      public int[] Lambda_Arr { get; set; }
      // public IEnumerable<int> Lambda_Arr { get; set; }
    }

    private readonly string streamName = "stream2";

    [TestMethod]
    public async Task Transform()
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

    [TestMethod]
    public async Task Filter()
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
    public async Task Reduce()
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
  }
}