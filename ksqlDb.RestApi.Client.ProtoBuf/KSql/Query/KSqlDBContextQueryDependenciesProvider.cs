using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ksqlDb.RestApi.Client.ProtoBuf.KSql.Query;

internal class KSqlDBContextQueryDependenciesProvider : ksqlDB.RestApi.Client.KSql.Query.Context.KSqlDBContextQueryDependenciesProvider
{
  public KSqlDBContextQueryDependenciesProvider(KSqlDBContextOptions kSqlDbContextOptions)
    : base(kSqlDbContextOptions)
  {
  }

  protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryProvider>();
    serviceCollection.TryAddSingleton(contextOptions.QueryParameters);

    base.OnConfigureServices(serviceCollection, contextOptions);
  }
}