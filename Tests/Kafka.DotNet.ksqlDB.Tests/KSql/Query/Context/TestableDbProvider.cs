using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kafka.DotNet.ksqlDB.KSql.Query;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ksqlDB.Api.Client.Tests.KSql.Query.Context
{
  public class TestableDbProvider<TValue> : KSqlDBContext
  {
    public TestableDbProvider(string ksqlDbUrl) : base(ksqlDbUrl)
    {
      InitMocks();
    }

    public TestableDbProvider(KSqlDBContextOptions contextOptions) : base(contextOptions)
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

    internal bool RegisterKSqlQueryGenerator { get; set; } = true;

    protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions options)
    {
      serviceCollection.AddSingleton(KSqlDbProviderMock.Object);

      if(RegisterKSqlQueryGenerator)
        serviceCollection.AddSingleton(KSqlQueryGenerator.Object);

      base.OnConfigureServices(serviceCollection, options);
    }
  }
}