using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

internal class KSqlDBContextQueryDependenciesProvider : KSqlDBContextDependenciesProvider
{
  public KSqlDBContextQueryDependenciesProvider(KSqlDBContextOptions kSqlDbContextOptions)
    : base(kSqlDbContextOptions)
  {
  }
    
  protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    base.OnConfigureServices(serviceCollection, contextOptions);

    serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryProvider>();
    serviceCollection.TryAddSingleton(contextOptions.QueryParameters);
  }
}