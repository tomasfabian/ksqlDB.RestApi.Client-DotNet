using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ksqlDB.Api.Client.Tests.Mocking
{
  public class TestableKSqlDBContext : KSqlDBContext
  {
    public TestableKSqlDBContext(string ksqlDbUrl) : base(ksqlDbUrl)
    {
    }

    public TestableKSqlDBContext(KSqlDBContextOptions contextOptions) : base(contextOptions)
    {
    }
    
    public readonly Mock<IKSqlDbProvider> KSqlDbProviderMock = new Mock<IKSqlDbProvider>();

    protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
    {
      serviceCollection.TryAddScoped<IKSqlDbProvider>(c => KSqlDbProviderMock.Object);
      
      base.OnConfigureServices(serviceCollection, contextOptions);
    }
  }

  [TestClass]
  public class KSqlDbTests
  {
    [TestMethod]
    public async Task GetById()
    {
      //Arrange
      var ksqlDbContextMock = new Mock<IKSqlDBContext>();
      var pullQueryMock = new Mock<IPullable<ElasticSearchEvent>>();
      var pullQueryProviderMock = new Mock<IPullQueryProvider>();

      pullQueryProviderMock.Setup(c => c.CreateQuery<ElasticSearchEvent>(It.IsAny<Expression>()))
        .Returns(pullQueryMock.Object);

      pullQueryMock.Setup(c => c.Provider)
        .Returns(pullQueryProviderMock.Object);

      pullQueryMock.Setup(c => c.Expression)
        .Returns(Expression.Constant(pullQueryMock.Object));

      pullQueryMock.Setup(c => c.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ElasticSearchEvent { Key = 42 });

      ksqlDbContextMock.Setup(c => c.CreatePullQuery<ElasticSearchEvent>("EventTopic"))
        .Returns(pullQueryMock.Object);

      var classUnderTest = new KSqlDb(ksqlDbContextMock.Object);
      
      //Act
      var elasticSearchEvent = await classUnderTest.GetByIdAsync(42);

      //Assert
      Assert.AreEqual(42, elasticSearchEvent.Key);
    }

    internal static async IAsyncEnumerable<ElasticSearchEvent> ElasticSearchEventsSource()
    {
      yield return new ElasticSearchEvent { Key = 1 };

      yield return new ElasticSearchEvent { Key = 2 };

      await Task.CompletedTask;
    }

    [TestMethod]
    public async Task Subscribe()
    {
      //Arrange
      var ksqlDbContext = new TestableKSqlDBContext(TestParameters.KsqlDBUrl);

      ksqlDbContext.KSqlDbProviderMock
        .Setup(c => c.Run<ElasticSearchEvent>(It.IsAny<QueryStreamParameters>(), It.IsAny<CancellationToken>()))
        .Returns(ElasticSearchEventsSource);
      
      var classUnderTest = new KSqlDb(ksqlDbContext);
      
      var semaphoreSlim = new SemaphoreSlim(0, 1);
      var receivedValues = new List<ElasticSearchEvent>();

      //Act
      var qbservable = classUnderTest.CreateElasticSearchEventQuery();

      var subscription = qbservable.Subscribe(value =>
      {
        receivedValues.Add(value);
      }, exception =>
      {
        semaphoreSlim.Release();
      }, () => semaphoreSlim.Release());

      await semaphoreSlim.WaitAsync();

      //Assert
      Assert.AreEqual(2, receivedValues.Count);

      using(subscription){}
    }
  }

  public interface IKSqlDb
  {
    Task<ElasticSearchEvent> GetByIdAsync(int id);
    IQbservable<ElasticSearchEvent> CreateElasticSearchEventQuery();
  }

  class KSqlDb : IKSqlDb
  {
    private readonly IKSqlDBContext context;

    public KSqlDb(IKSqlDBContext context)
    {
      this.context = context;
    }

    public async Task<ElasticSearchEvent> GetByIdAsync(int id)
    {
      var response = await context.CreatePullQuery<ElasticSearchEvent>("EventTopic")
        .Where(c => c.Key == id)
        .FirstOrDefaultAsync();

      return response;
    }

    public IQbservable<ElasticSearchEvent> CreateElasticSearchEventQuery()
    {
      var query = context.CreateQueryStream<ElasticSearchEvent>()
        .Where(p => p.Key != 33);

      return query;
    }
  }

  public class ElasticSearchEvent
  {
    public int Key { get; set; }
  }
}