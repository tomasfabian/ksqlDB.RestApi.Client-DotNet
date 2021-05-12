using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kafka.DotNet.ksqlDB.KSql.Query;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.Query.Context
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
      KSqldbProviderMock.Setup(c => c.Run<TValue>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
        .Returns(GetAsyncEnumerable);
    }

    protected virtual IAsyncEnumerable<TValue> GetAsyncEnumerable()
    {
      return new List<TValue>().ToAsyncEnumerable();
    }

    public readonly Mock<IKSqlDbProvider> KSqldbProviderMock = new Mock<IKSqlDbProvider>();
    public readonly Mock<IKSqlQueryGenerator> KSqlQueryGenerator = new Mock<IKSqlQueryGenerator>();

    protected bool RegisterKSqlQueryGenerator { get; set; } = true;

    protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions options)
    {
      serviceCollection.AddSingleton(KSqldbProviderMock.Object);

      if(RegisterKSqlQueryGenerator)
        serviceCollection.AddSingleton(KSqlQueryGenerator.Object);

      base.OnConfigureServices(serviceCollection, options);
    }
  }
}