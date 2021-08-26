using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Windows;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.Tests.Helpers;
using Kafka.DotNet.ksqlDB.Tests.Pocos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Linq
{
  [TestClass]
  public class QbservableGroupByExtensionsTests : TestBase
  {
    private IQbservable<City> CreateQbservable()
    {
      var context = new TestableDbProvider(TestParameters.KsqlDBUrl);
      
      context.KSqldbProviderMock.Setup(c => c.Run<int>(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(GetTestValues);
      context.KSqldbProviderMock.Setup(c => c.Run<long>(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(GetDecimalTestValues);

      return context.CreateQueryStream<City>();
    }
          
    TestScheduler testScheduler;

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      testScheduler = new TestScheduler();
    }

    #region Count

    [TestMethod]
    public void GroupByAndCount_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => g.Count());

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT COUNT(*) FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCount_Named_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => new {Count = g.Count()});     

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT COUNT(*) Count FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCountByKey_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => new { RegionCode = g.Key, Count = g.Count()});

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT RegionCode, COUNT(*) Count FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCountHaving_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Having(c => c.Count() > 2)
        .Select(g => new { RegionCode = g.Key, Count = g.Count()});

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT RegionCode, COUNT(*) Count FROM Cities GROUP BY RegionCode HAVING Count(*) > 2 EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndCount_Subscribe_ReceivesValues()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => g.Count());

      bool valuesWereReceived = false;

      //Act
      var subscription = grouping.SubscribeOn(testScheduler)
        .ObserveOn(testScheduler)
        .Subscribe(c => { valuesWereReceived = true; });

      testScheduler.Start();

      //Assert
      valuesWereReceived.Should().BeTrue();

      subscription.Dispose();
    }

    #endregion

    #region Sum

    [TestMethod]
    public void GroupByAndSum_Subscribe_ReceivesValues()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => g.Sum(c => c.Citizens));

      bool valuesWereReceived = false;

      //Act
      var subscription = grouping.SubscribeOn(testScheduler).ObserveOn(testScheduler).Subscribe(c => { valuesWereReceived = true; });
      testScheduler.Start();

      //Assert
      valuesWereReceived.Should().BeTrue();

      subscription.Dispose();
    }

    [TestMethod]
    public void GroupByAndSum_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => g.Sum(c => c.Citizens));

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT SUM(Citizens) FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    [TestMethod]
    public void GroupByAndSumWithColumn_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => new { RegionCode = g.Key, MySum = g.Sum(c => c.Citizens)});

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT RegionCode, SUM(Citizens) MySum FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    #endregion

    #region Avg

    [TestMethod]
    public void GroupByAndAvg_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => g.Avg(c => c.Citizens));

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT Avg(Citizens) FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    #endregion

    #region GroupBy

    [TestMethod]
    public void GroupByCompoundKey_BuildKSql_PrintsQuery()
    {
      //Arrange
      var context = new TestableDbProvider(TestParameters.KsqlDBUrl);

      //https://kafka-tutorials.confluent.io/finding-distinct-events/ksql.html
      var grouping = context.CreateQueryStream<Click>()
        .GroupBy(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP })
        .WindowedBy(new TimeWindows(Duration.OfMinutes(2)))
        .Having(c => c.Count(g => c.Key.IP_ADDRESS) == 1)
        .Select(g => new { g.Key.IP_ADDRESS, g.Key.URL, g.Key.TIMESTAMP })
        .Take(3);

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      string expectedKSql = @"SELECT IP_ADDRESS, URL, TIMESTAMP FROM Clicks WINDOW TUMBLING (SIZE 2 MINUTES) GROUP BY IP_ADDRESS, URL, TIMESTAMP HAVING COUNT(IP_ADDRESS) = 1 EMIT CHANGES LIMIT 3;";
      
      ksql.Should().BeEquivalentTo(expectedKSql);
    }

    #endregion

    #region Min

    [TestMethod]
    public void GroupByAndMin_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => g.Min(c => c.Citizens));

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT MIN(Citizens) FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    #endregion

    #region Max

    [TestMethod]
    public void GroupByAndMax_BuildKSql_PrintsQuery()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(g => g.Max(c => c.Citizens));

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo("SELECT MAX(Citizens) FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    #endregion

    #region Cast
    
    [TestMethod]
    public void ConvertToString_Cast()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(c => new { RegionCode = c.Key, Count = Convert.ToString(c.Count()) });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo(@"SELECT RegionCode, CAST(COUNT(*) AS VARCHAR) Count FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }
    
    [TestMethod]
    public void ToString_Cast()
    {
      //Arrange
      var grouping = CreateQbservable()
        .GroupBy(c => c.RegionCode)
        .Select(c => new { RegionCode = c.Key, Count = c.Count().ToString() });

      //Act
      var ksql = grouping.ToQueryString();

      //Assert
      ksql.Should().BeEquivalentTo(@"SELECT RegionCode, CAST(COUNT(*) AS VARCHAR) Count FROM Cities GROUP BY RegionCode EMIT CHANGES;");
    }

    #endregion
    
    public static async IAsyncEnumerable<int> GetTestValues()
    {
      yield return 1;

      yield return 2;
      
      yield return 3;
      
      await Task.CompletedTask;
    }

    public static async IAsyncEnumerable<long> GetDecimalTestValues()
    {
      yield return 1;

      yield return 2;
      
      yield return 3;
      
      await Task.CompletedTask;
    }  

    protected async IAsyncEnumerable<City> GetCities()
    {
      yield return new City { RegionCode = "A1" };

      yield return new City { RegionCode = "B1" };

      yield return new City { RegionCode = "A1" };
      
      await Task.CompletedTask;
    }

    public class City
    {
      public string RegionCode { get; set; }
      public long Citizens { get; set; }
    }
  }

  class TestableDbProvider : TestableDbProvider<QbservableGroupByExtensionsTests.City>
  {
    private readonly IHttpClientFactory httpClientFactory;

    public TestableDbProvider(string ksqlDbUrl) 
      : base(ksqlDbUrl)
    {
      RegisterKSqlQueryGenerator = false;
    }

    public TestableDbProvider(string ksqlDbUrl, IHttpClientFactory httpClientFactory) 
      : base(ksqlDbUrl)
    {
      this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

      RegisterKSqlQueryGenerator = false;    
      
      KSqlDBQueryContext.Configure(sc =>
      {
        sc.AddSingleton(httpClientFactory);
      });
    }

    public TestableDbProvider(KSqlDBContextOptions contextOptions) 
      : base(contextOptions)
    {
      RegisterKSqlQueryGenerator = false;
    }
  }
}