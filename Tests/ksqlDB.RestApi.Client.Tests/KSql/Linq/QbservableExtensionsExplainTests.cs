using FluentAssertions;
using ksqlDB.Api.Client.Tests.Fakes.Http;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.Api.Client.Tests.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.Api.Client.Tests.KSql.Linq;

[TestClass]
public class QbservableExtensionsExplainTests : TestBase
{
  private TestableDbProviderForExplain dbProvider = null!;

  [TestInitialize]
  public void Initialize()
  {
    var httpClientFactory = Mock.Of<IHttpClientFactory>();
    var httpClient = FakeHttpClient.CreateWithResponse(response);

    Mock.Get(httpClientFactory).Setup(c => c.CreateClient()).Returns(() => httpClient);

    dbProvider = new TestableDbProviderForExplain(TestParameters.KsqlDBUrl, httpClientFactory);
  }

  private readonly string response =
    @"[{""@type"":""queryDescription"",""statementText"":""EXPLAIN SELECT * FROM Movies EMIT CHANGES;"",""queryDescription"":{""id"":""_confluent-ksql-ksql-connect-clustertransient_3226006621890769790_1631632793312"",""statementText"":""SELECT * FROM Movies EMIT CHANGES;"",""windowType"":null,""fields"":[{""name"":""TITLE"",""schema"":{""type"":""STRING"",""fields"":null,""memberSchema"":null},""type"":""KEY""},{""name"":""TITLE"",""schema"":{""type"":""STRING"",""fields"":null,""memberSchema"":null}},{""name"":""ID"",""schema"":{""type"":""INTEGER"",""fields"":null,""memberSchema"":null}},{""name"":""RELEASE_YEAR"",""schema"":{""type"":""INTEGER"",""fields"":null,""memberSchema"":null}}],""sources"":[""MOVIES""],""sinks"":[],""topology"":""Topologies:\n   Sub-topology: 0\n    Source: KSTREAM-SOURCE-0000000001 (topics: [movies])\n      --> KTABLE-SOURCE-0000000002\n    Processor: KTABLE-SOURCE-0000000002 (stores: [])\n      --> KTABLE-MAPVALUES-0000000003\n      <-- KSTREAM-SOURCE-0000000001\n    Processor: KTABLE-MAPVALUES-0000000003 (stores: [KsqlTopic-Reduce])\n      --> KTABLE-TRANSFORMVALUES-0000000004\n      <-- KTABLE-SOURCE-0000000002\n    Processor: KTABLE-TRANSFORMVALUES-0000000004 (stores: [])\n      --> Project\n      <-- KTABLE-MAPVALUES-0000000003\n    Processor: Project (stores: [])\n      --> KTABLE-TOSTREAM-0000000006\n      <-- KTABLE-TRANSFORMVALUES-0000000004\n    Processor: KTABLE-TOSTREAM-0000000006 (stores: [])\n      --> KSTREAM-FOREACH-0000000007\n      <-- Project\n    Processor: KSTREAM-FOREACH-0000000007 (stores: [])\n      --> none\n      <-- KTABLE-TOSTREAM-0000000006\n\n"",""executionPlan"":"" > [ PROJECT ] | Schema: TITLE STRING KEY, TITLE STRING, ID INTEGER, RELEASE_YEAR INTEGER | Logger: 3226006621890769790.Project\n\t\t > [ SOURCE ] | Schema: TITLE STRING KEY, ID INTEGER, RELEASE_YEAR INTEGER, ROWTIME BIGINT, TITLE STRING | Logger: 3226006621890769790.KsqlTopic.Source\n"",""overriddenProperties"":{},""ksqlHostQueryStatus"":{},""queryType"":""PUSH"",""queryErrors"":[],""tasksMetadata"":[],""state"":null},""warnings"":[]}]";

  [TestMethod]
  public async Task ExplainAsync()
  {
    //Arrange
    var query = dbProvider.CreateQueryStream<string>();

    //Act
    var description = await query.ExplainAsync();

    //Assert
    description[0].StatementText.Should().Be("EXPLAIN SELECT * FROM Movies EMIT CHANGES;");
    description[0].QueryDescription.QueryType.Should().Be("PUSH");
  }

  [TestMethod]
  public async Task ExplainAsStringAsync()
  {
    //Arrange
    var query = dbProvider.CreateQueryStream<string>();

    //Act
    var description = await query.ExplainAsStringAsync();

    //Assert
    description.Should().Be(response);
  }

  [TestMethod]
  public void CreateExplainStatement()
  {
    //Arrange
    var query = dbProvider.CreateQueryStream<string>().Where(c => c == "ET");

    //Act
    var explainStatement = QbservableExtensions.CreateExplainStatement(query as KStreamSet<string>);

    //Assert
    explainStatement.Should().Be(@"EXPLAIN SELECT * FROM Strings
WHERE c = 'ET' EMIT CHANGES;");
  }

  class TestableDbProviderForExplain : TestableDbProvider<QbservableGroupByExtensionsTests.City>
  {
    private readonly IHttpClientFactory httpClientFactory;

    public TestableDbProviderForExplain(string ksqlDbUrl, IHttpClientFactory httpClientFactory) 
      : base(ksqlDbUrl)
    {
      this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

      RegisterKSqlQueryGenerator = false;
    }

    protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions options)
    {
      serviceCollection.AddSingleton(httpClientFactory);

      base.OnConfigureServices(serviceCollection, options);
    }
  }
}
