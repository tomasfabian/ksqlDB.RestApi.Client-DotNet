using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
#if !NETSTANDARD
using KSqlDbQueryStreamProvider = ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi.KSqlDbQueryStreamProvider;
#endif

namespace ksqlDb.RestApi.Client.ProtoBuf.KSql.Query.Context;

public class ProtoBufKSqlDbContext : KSqlDBContext
{
  public ProtoBufKSqlDbContext(string ksqlDbUrl, ILoggerFactory? loggerFactory = null)
    : this(new KSqlDBContextOptions(ksqlDbUrl), loggerFactory)
  {
  }

  public ProtoBufKSqlDbContext(KSqlDBContextOptions contextOptions, ILoggerFactory? loggerFactory = null)
    : base(contextOptions, loggerFactory)
  {
    KSqlDBQueryContext = new KSqlDBContextQueryDependenciesProvider(contextOptions);
  }

#if !NETSTANDARD
  protected override void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryStreamProvider>();

    base.OnConfigureServices(serviceCollection, contextOptions);
  }
#endif

}
