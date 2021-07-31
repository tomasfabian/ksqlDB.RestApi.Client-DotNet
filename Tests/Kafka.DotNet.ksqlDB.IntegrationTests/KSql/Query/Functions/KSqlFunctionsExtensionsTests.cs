using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.IntegrationTests.Models.Movies;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Query.Functions
{
  [TestClass]
  public class KSqlFunctionsExtensionsTests : Linq.IntegrationTests
  {
    private static MoviesProvider moviesProvider;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      RestApiProvider = KSqlDbRestApiProvider.Create();
      
      moviesProvider = new MoviesProvider(RestApiProvider);
      await moviesProvider.CreateTablesAsync();

      await moviesProvider.InsertMovieAsync(MoviesProvider.Movie1);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
      await moviesProvider.DropTablesAsync();

      moviesProvider = null;
    }

    private string MoviesTableName => MoviesProvider.MoviesTableName;

    [TestMethod]
    public async Task DateToString()
    {
      await DateToStringTest(Context.CreateQueryStream<Movie>(MoviesTableName));
    }

    [TestMethod]
    public async Task DateToString_QueryEndPoint()
    {
      await DateToStringTest(Context.CreateQuery<Movie>(MoviesTableName));
    }

    public async Task DateToStringTest(IQbservable<Movie> querySource)
    {
      //Arrange
      int expectedItemsCount = 1;
      
      int epochDays = 18672;
      string format = "yyyy-MM-dd";
      Expression<Func<Movie, string>> expression = _ => KSqlFunctions.Instance.DateToString(epochDays, format);
      
      //Act
      var source = querySource
        .Select(expression)
        //.Select(c => new { DTS = KSqlFunctions.Instance.DateToString(epochDays, format) })
        .ToAsyncEnumerable();
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Should().BeEquivalentTo("2021-02-14");
    }

    [TestMethod]
    public async Task Entries()
    {
      await EntriesTest(Context.CreateQueryStream<Movie>(MoviesTableName));
    }

    [TestMethod]
    public async Task Entries_QueryEndPoint()
    {
      await EntriesTest(Context.CreateQuery<Movie>(MoviesTableName));
    }
    
    public async Task EntriesTest(IQbservable<Movie> querySource)
    {
      //Arrange
      int expectedItemsCount = 1;
      
      bool sorted = true;
      
      //Act
      var source = querySource
        .Select(c => new { Col = KSqlFunctions.Instance.Entries(new Dictionary<string, string>()
        {
          { "a", "value" }
        }, sorted)})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col[0].K.Should().BeEquivalentTo("a");
      actualValues[0].Col[0].V.Should().BeEquivalentTo("value");
    }

    [TestMethod]
    public async Task ArrayIntersect()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = KSqlFunctions.Instance.ArrayIntersect(new [] { 1, 2 }, new []{ 1 } )})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Length.Should().Be(1);
      actualValues[0].Col[0].Should().Be(1);
    }

    [TestMethod]
    public async Task ArrayJoin()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = KSqlFunctions.Instance.ArrayJoin(new [] { 1, 2 }, ";" )})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues.Count.Should().Be(1);
      actualValues[0].Col.Should().Be(@"1;2");
    }

    [TestMethod]
    public async Task ArrayLength()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = KSqlFunctions.Instance.ArrayLength(new [] { 1, 2 } )})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues.Count.Should().Be(1);
      actualValues[0].Col.Should().Be(2);
    }
    
    [TestMethod]
    public async Task ArrayMin()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = KSqlFunctions.Instance.ArrayMin(new [] { 1, 2 } )})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues.Count.Should().Be(1);
      actualValues[0].Col.Should().Be(1);
    }

    [TestMethod]
    [Ignore("Cannot construct an array with all NULL elements")]
    public async Task ArrayMin_Null()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = KSqlFunctions.Instance.ArrayMin(new string [] { null })})
        .ToAsyncEnumerable();

      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues.Count.Should().Be(1);
      actualValues[0].Col.Should().BeNull();
    }

    [TestMethod]
    public async Task ArrayLength_NullValue()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = KSqlFunctions.Instance.ArrayLength(null as string[])})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues.Count.Should().Be(1);
      actualValues[0].Col.Should().BeNull();
    }
    
    [TestMethod]
    public async Task ArrayRemove()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = KSqlFunctions.Instance.ArrayRemove(new [] { 1, 2 }, 2)})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues.Count.Should().Be(1);
      actualValues[0].Col.Length.Should().Be(1);
    }
    
    [TestMethod]
    public async Task ArraySort()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = K.Functions.ArraySort(new int?[]{ 3, null, 1}, ListSortDirection.Ascending)})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      CollectionAssert.AreEquivalent(new int?[] { 1, 3, null}, actualValues[0].Col);
    }
    
    [TestMethod]
    public async Task ArrayUnion()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = K.Functions.ArrayUnion(new int?[]{ 3, null, 1}, new int?[]{ 4, null})})
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      CollectionAssert.AreEquivalent(new int?[] { 3, null, 1, 4}, actualValues[0].Col);
    }
    
    [TestMethod]
    public async Task Concat()
    {
      //Arrange
      int expectedItemsCount = 1;
      string message = "_Hello";

      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = K.Functions.Concat(c.Title, message), ColWS = K.Functions.ConcatWS(" - ", c.Title, message) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Should().Be($"{MoviesProvider.Movie1.Title}{message}");
      actualValues[0].ColWS.Should().Be($"{MoviesProvider.Movie1.Title} - {message}");
    }

    [TestMethod]
    public async Task AsMap()
    {
      //Arrange
      int expectedItemsCount = 1;

      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = K.Functions.AsMap(new []{ "1", "2" }, new []{ 11, 22 }) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col["1"].Should().Be(11);
    }
    
    [TestMethod]
    public async Task JsonArrayContains()
    {
      //Arrange
      int expectedItemsCount = 1;

      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = K.Functions.JsonArrayContains("[1, 2, 3]", 2) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col.Should().BeTrue();
    }
    
    [TestMethod]
    public async Task MapKeys()
    {
      //Arrange
      int expectedItemsCount = 1;
      var map = new Dictionary<string, int>
      {
        {"apple", 10},
        {"banana", 20}
      };

      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Col = K.Functions.MapKeys(new Dictionary<string, int>
        {
          {"apple", 10},
          {"banana", 20}
        }) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Col[0].Should().Be("banana");
      actualValues[0].Col[1].Should().Be("apple");
    }
    
    [TestMethod]
    public async Task ExtractJsonField()
    {
      //Arrange
      int expectedItemsCount = 1;

      string json =
        "{\r\n   \"log\": {\r\n      \"cloud\": \"gcp836Csd\",\r\n      \"app\": \"ksProcessor\",\r\n      \"instance\": 4\r\n   }\r\n}";

      string jsonPath = "$.log.cloud";

      //Act
      var source = Context.CreateQuery<Movie>(MoviesTableName)
        .Select(c => new { Extracted = K.Functions.ExtractJsonField(json, jsonPath) })
        .ToAsyncEnumerable();
      
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      actualValues[0].Extracted.Should().Be("gcp836Csd");
    }
  }
}