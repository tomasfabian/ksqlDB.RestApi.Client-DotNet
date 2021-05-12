using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.IntegrationTests.Models;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Windows;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq
{
  [TestClass]
  [TestCategory("Integration")]
  public class QbservableExtensionsTests : IntegrationTests
  {
    protected static string StreamName = "tweetsTest";
    private static string topicName = "tweetsTestTopic";

    private static Tweet Tweet1 => TweetsProvider.Tweet1;

    private static Tweet Tweet2 => TweetsProvider.Tweet2;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      await InitializeDatabase();
    }

    protected static async Task InitializeDatabase()
    {
      RestApiProvider = KSqlDbRestApiProvider.Create();

      var tweetsProvider = new TweetsProvider(RestApiProvider);
      var result = await tweetsProvider.CreateTweetsStream(StreamName, topicName);
      result.Should().BeTrue();

      result = await tweetsProvider.InsertTweetAsync(Tweet1, StreamName);

      result.Should().BeTrue();

      result = await tweetsProvider.InsertTweetAsync(Tweet2, StreamName);

      result.Should().BeTrue();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
      var result = await RestApiProvider.DropStreamAndTopic(StreamName);
    }

    protected virtual ksqlDB.KSql.Linq.IQbservable<Tweet> QuerySource =>
      Context.CreateQueryStream<Tweet>(StreamName);

    [TestMethod]
    public async Task Select()
    {
      //Arrange
      int expectedItemsCount = 2;
      
      var source = QuerySource
        .ToAsyncEnumerable();
      
      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      var expectedValues = new List<Tweet>
      {
        Tweet1, Tweet2
      };
      
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      CollectionAssert.AreEqual(expectedValues, actualValues);
    }

    [TestMethod]
    public async Task Take()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      var source = QuerySource
        .Take(expectedItemsCount)
        .ToAsyncEnumerable();
      
      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      var expectedValues = new List<Tweet>
      {
        Tweet1
      };

      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      CollectionAssert.AreEqual(expectedValues, actualValues);
    }

    [TestMethod]
    public async Task Where_MessageWasFiltered()
    {
      //Arrange
      int expectedItemsCount = 1;
      
      var source = QuerySource
        .Where(p => p.Message != "Hello world")
        .ToAsyncEnumerable();
      
      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);
      
      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
      Assert.AreEqual(actualValues[0].Message, Tweet2.Message);
    }

    [TestMethod]
    public async Task Subscribe()
    {
      //Arrange
      var semaphore = new SemaphoreSlim(initialCount: 0, 1);
      var actualValues = new List<Tweet>();

      int expectedItemsCount = 2;
      
      var source = QuerySource;

      //Act
      using var subscription = source.Take(expectedItemsCount).Subscribe(c => actualValues.Add(c), e => semaphore.Release(), () => semaphore.Release());
      await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
    }

    [TestMethod]
    public async Task ToObservable()
    {
      //Arrange
      var semaphore = new SemaphoreSlim(initialCount: 0, 1);
      var actualValues = new List<Tweet>();

      int expectedItemsCount = 2;
      
      var source = QuerySource
        .ToObservable();

      //Act
      using var subscription = source.Take(expectedItemsCount).Subscribe(c => actualValues.Add(c), e => semaphore.Release(), () => semaphore.Release());
      await semaphore.WaitAsync(TimeSpan.FromSeconds(4));

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
    }

    [TestMethod]
    public async Task GroupBy()
    {
      //Arrange
      int expectedItemsCount = 2;

      var source = QuerySource
        .GroupBy(c => c.Id)
        .Select(g => new {Id = g.Key, Count = g.Count(c => c.Message)})
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);

      Assert.AreEqual(1, actualValues[0].Count);
      Assert.AreEqual(Tweet1.Id, actualValues[0].Id);

      Assert.AreEqual(1, actualValues[1].Count);
      Assert.AreEqual(Tweet2.Id, actualValues[1].Id);
    }

    [TestMethod]
    public async Task Having()
    {
      //Arrange
      int expectedItemsCount = 2;

      var source = QuerySource
        .GroupBy(c => c.Id)
        .Having(c => c.Count(g => g.Message) == 1)
        .Select(g => new {Id = g.Key, Count = g.Count(c => c.Message)})
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);

      Assert.AreEqual(1, actualValues[0].Count);
      Assert.AreEqual(Tweet1.Id, actualValues[0].Id);

      Assert.AreEqual(1, actualValues[1].Count);
      Assert.AreEqual(Tweet2.Id, actualValues[1].Id);
    }

    [TestMethod]
    public async Task WindowedBy()
    {
      //Arrange
      int expectedItemsCount = 2;

      var source = QuerySource
        .GroupBy(c => c.Id)
        .WindowedBy(new TimeWindows(Duration.OfMilliseconds(100)))
        .Select(g => new {Id = g.Key, Count = g.Count(c => c.Message)})
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);

      Assert.AreEqual(1, actualValues[0].Count);
      Assert.AreEqual(Tweet1.Id, actualValues[0].Id);

      Assert.AreEqual(1, actualValues[1].Count);
      Assert.AreEqual(Tweet2.Id, actualValues[1].Id);
    }
    
    [TestMethod]
    public async Task QueryRawKSql()
    {
      //Arrange
      int expectedItemsCount = 2;
      
      string ksql = @"SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

      QueryParameters queryParameters = new QueryParameters
      {
        Sql = ksql,
        [QueryParameters.AutoOffsetResetPropertyName] = "earliest",
      };

      var source = Context.CreateQuery<Tweet>(queryParameters);
      
      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
    }
    
    [TestMethod]
    public async Task QueryStreamRawKSql()
    {
      //Arrange
      int expectedItemsCount = 2;
      
      string ksql = @"SELECT * FROM tweetsTest EMIT CHANGES LIMIT 2;";

      QueryStreamParameters queryStreamParameters = new QueryStreamParameters
      {
        Sql = ksql,
        [QueryStreamParameters.AutoOffsetResetPropertyName] = "earliest",
      };

      var source = Context.CreateQueryStream<Tweet>(queryStreamParameters);
      
      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      Assert.AreEqual(expectedItemsCount, actualValues.Count);
    }
  }
}