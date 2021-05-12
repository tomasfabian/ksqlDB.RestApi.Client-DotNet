using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context
{
  internal class KSqlDBContextQueryDependenciesProvider : KSqlDBContextDependenciesProvider
  {
    protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
    {
      base.OnConfigureServices(serviceCollection, contextOptions);

      serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryProvider>();
      serviceCollection.TryAddSingleton(contextOptions.QueryParameters);
    }
  }
}