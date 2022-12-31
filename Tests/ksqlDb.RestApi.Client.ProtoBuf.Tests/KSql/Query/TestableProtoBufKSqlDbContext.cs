using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.Query.Context;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.Query;

public class TestableProtoBufKSqlDbContext<TValue> : ProtoBufKSqlDbContext
{
  public TestableProtoBufKSqlDbContext(string ksqlDbUrl)
    : base(ksqlDbUrl)
  {
    InitMocks();
  }

  public TestableProtoBufKSqlDbContext(KSqlDBContextOptions contextOptions) : base(contextOptions)
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

  internal bool RegisterKSqlQueryGenerator { get; set; } = true;
  protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions options)
  {
    serviceCollection.AddSingleton(KSqlDbProviderMock.Object);
    serviceCollection.AddSingleton(KSqlDbRestApiClientMock.Object);

    if (RegisterKSqlQueryGenerator)
      serviceCollection.AddSingleton(KSqlQueryGenerator.Object);

    base.OnConfigureServices(serviceCollection, options);
  }
}