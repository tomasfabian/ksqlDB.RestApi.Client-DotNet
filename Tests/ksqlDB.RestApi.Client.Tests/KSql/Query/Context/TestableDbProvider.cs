using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ksqlDb.RestApi.Client.Tests.KSql.Query.Context;

public class TestableDbProvider<TValue> : KSqlDBContext
{
  public TestableDbProvider(string ksqlDbUrl)
    : base(ksqlDbUrl)
  {
    InitMocks();
  }

  public TestableDbProvider(KSqlDBContextOptions contextOptions)
    : base(contextOptions)
  {
    InitMocks();
  }

  public TestableDbProvider(KSqlDBContextOptions contextOptions, ModelBuilder modelBuilder)
    : base(contextOptions, modelBuilder)
  {
    InitMocks();
  }

  private void InitMocks()
  {
    KSqlDbProviderMock.Setup(c => c.Run<TValue>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
      .Returns(GetAsyncEnumerable);
  }

  protected virtual IAsyncEnumerable<TValue> GetAsyncEnumerable()
  {
    return new List<TValue>().ToAsyncEnumerable();
  }

  public readonly Mock<IKSqlDbProvider> KSqlDbProviderMock = new();
  public readonly Mock<IKSqlQueryGenerator> KSqlQueryGenerator = new();
  public readonly Mock<IKSqlDbRestApiClient> KSqlDbRestApiClientMock = new();

  internal bool RegisterKSqlDbProvider { get; set; } = true;
  internal bool RegisterKSqlQueryGenerator { get; set; } = true;
  internal bool RegisterKSqlDbRestApiClient { get; set; } = true;

  protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions options)
  {
    if(RegisterKSqlDbProvider)
      serviceCollection.AddSingleton(KSqlDbProviderMock.Object);
    if(RegisterKSqlDbRestApiClient)
      serviceCollection.AddSingleton(KSqlDbRestApiClientMock.Object);
    if(RegisterKSqlQueryGenerator)
      serviceCollection.AddSingleton(KSqlQueryGenerator.Object);

    base.OnConfigureServices(serviceCollection, options);
  }
}
