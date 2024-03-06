using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDb.RestApi.Client.Tests.Fakes.Http;
using ksqlDb.RestApi.Client.Tests.KSql.Query.Context;
using ksqlDb.RestApi.Client.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using UnitTests;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.KSql.Linq;

public class QbservableGroupByExtensionsTests : TestBase
{
  private static IQbservable<City> CreateQbservable()
  {
    var context = new TestableDbProvider(TestParameters.KsqlDbUrl);
      
    context.KSqlDbProviderMock.Setup(c => c.Run<int>(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(GetTestValues);
    context.KSqlDbProviderMock.Setup(c => c.Run<long>(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(GetDecimalTestValues);

    return context.CreateQueryStream<City>();
  }

  private TestScheduler testScheduler = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    testScheduler = new TestScheduler();
  }

  #region Count

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
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

  [Test]
  public void GroupByCompoundKey_BuildKSql_PrintsQuery()
  {
    //Arrange
    var context = new TestableDbProvider(TestParameters.KsqlDbUrl);

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
    string expectedKSql = "SELECT IP_ADDRESS, URL, TIMESTAMP FROM Clicks WINDOW TUMBLING (SIZE 2 MINUTES) GROUP BY IP_ADDRESS, URL, TIMESTAMP HAVING COUNT(IP_ADDRESS) = 1 EMIT CHANGES LIMIT 3;";
      
    ksql.Should().BeEquivalentTo(expectedKSql);
  }

  [Test]
  public void GroupByNestedProperty_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = CreateQbservable()
      .GroupBy(c => c.State.Name)
      .Select(g => new { g.Source.State.Name, num_times = g.Count()});

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT State->Name, COUNT(*) num_times FROM Cities GROUP BY State->Name EMIT CHANGES;");
  }

  [Test]
  public void GroupByDeeplyNestedProperty_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = CreateQbservable()
      .GroupBy(c => c.State.Nested.Version)
      .Select(g => new { g.Source.State.Nested.Version, num_times = g.Count()});

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT State->Nested->Version, COUNT(*) num_times FROM Cities GROUP BY State->Nested->Version EMIT CHANGES;");
  }

  [Test]
  public void GroupByNestedPropertyWithSameAlias_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = CreateQbservable()
      .GroupBy(c => c.State.Name)
      .Select(g => new { Name = g.Source.State.Name, num_times = g.Count()});

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT State->Name, COUNT(*) num_times FROM Cities GROUP BY State->Name EMIT CHANGES;");
  }

  [Test]
  public void GroupByNestedPropertyWithDifferentAlias_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = CreateQbservable()
      .GroupBy(c => c.State.Name)
      .Select(g => new { MyAlias = g.Source.State.Name, num_times = g.Count()});

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT State->Name AS MyAlias, COUNT(*) num_times FROM Cities GROUP BY State->Name EMIT CHANGES;");
  }

  [Test]
  public void GroupByAnonymousType_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = CreateQbservable()
      .GroupBy(c => new { c.RegionCode, c.State.Name })
      .Select(g => new { g.Source.RegionCode, g.Source.State.Name, num_times = g.Count()});

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT RegionCode, State->Name, COUNT(*) num_times FROM Cities GROUP BY RegionCode, State->Name EMIT CHANGES;");
  }

  [Test]
  public void GroupByQuerySyntax_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = 
      from city in CreateQbservable()
      where city.RegionCode != "xx"
      group city by city.State.Name into g
      select new
      {
        g.Source.RegionCode,
        g.Source.State.Name,
        num_times = g.Count()
      };

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@"SELECT RegionCode, STATE->Name, COUNT(*) num_times FROM Cities
WHERE RegionCode != 'xx' GROUP BY State->Name EMIT CHANGES;".ReplaceLineEndings());
  }

  [Test]
  public void GroupByQuerySyntaxWithLimit_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = 
      (from city in CreateQbservable()
        where city.RegionCode != "xx"
        group city by city.State.Name into g
        select new
        {
          g.Source.RegionCode,
          g.Source.State.Name,
          num_times = g.Count()
        })
      .Take(2);

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@"SELECT RegionCode, STATE->Name, COUNT(*) num_times FROM Cities
WHERE RegionCode != 'xx' GROUP BY State->Name EMIT CHANGES LIMIT 2;".ReplaceLineEndings());
  }

  [Test]
  public void GroupNewByQuerySyntax_BuildKSql_PrintsQuery()
  {
    //Arrange
    var grouping = 
      from city in CreateQbservable()
      where city.RegionCode != "xx"
      group new { city.RegionCode } by city.RegionCode into g
      select new
      {
        g.Source.RegionCode,
        num_times = g.Count()
      };

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo(@"SELECT RegionCode, COUNT(*) num_times FROM Cities
WHERE RegionCode != 'xx' GROUP BY RegionCode EMIT CHANGES;".ReplaceLineEndings());
  }

  #endregion

  #region Min

  [Test]
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

  [Test]
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
    
  [Test]
  public void ConvertToString_Cast()
  {
    //Arrange
    var grouping = CreateQbservable()
      .GroupBy(c => c.RegionCode)
      .Select(c => new { RegionCode = c.Key, Count = Convert.ToString(c.Count()) });

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT RegionCode, CAST(COUNT(*) AS VARCHAR) Count FROM Cities GROUP BY RegionCode EMIT CHANGES;");
  }
    
  [Test]
  public void ToString_Cast()
  {
    //Arrange
    var grouping = CreateQbservable()
      .GroupBy(c => c.RegionCode)
      .Select(c => new { RegionCode = c.Key, Count = c.Count().ToString() });

    //Act
    var ksql = grouping.ToQueryString();

    //Assert
    ksql.Should().BeEquivalentTo("SELECT RegionCode, CAST(COUNT(*) AS VARCHAR) Count FROM Cities GROUP BY RegionCode EMIT CHANGES;");
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

  public class City
  {
    public string RegionCode { get; set; } = null!;
    public long Citizens { get; set; }
    public State State { get; set; } = null!;
    public int[] Values { get; set; } = null!;
  }

  public record State
  {
    public string Name { get; set; } = null!;
    public Nested Nested { get; set; } = null!;
  }

  public record Nested
  {
    public string Version { get; set; } = null!;
  }
}

class TestableDbProvider : TestableDbProvider<QbservableGroupByExtensionsTests.City>
{
  public TestableDbProvider(string ksqlDbUrl) 
    : base(ksqlDbUrl)
  {
    RegisterKSqlQueryGenerator = false;
  }

  public TestableDbProvider(string ksqlDbUrl, string httpResponse) 
    : base(ksqlDbUrl)
  {
    RegisterKSqlQueryGenerator = false;    
      
    KSqlDBQueryContext.Configure(sc =>
    {
      sc.AddHttpClient<IHttpV1ClientFactory, HttpClientFactory>(c => c.BaseAddress = new Uri(ksqlDbUrl))
        .AddHttpMessageHandler(_ => FakeHttpClient.CreateDelegatingHandler(httpResponse).Object);

      sc.AddHttpClient<IHttpClientFactory, HttpClientFactory>(c => c.BaseAddress = new Uri(ksqlDbUrl))
        .AddHttpMessageHandler(_ => FakeHttpClient.CreateDelegatingHandler(httpResponse).Object);

      // sc.AddSingleton(httpClientFactory);
    });
  }

  public TestableDbProvider(KSqlDBContextOptions contextOptions) 
    : base(contextOptions)
  {
    RegisterKSqlQueryGenerator = false;
  }
}
