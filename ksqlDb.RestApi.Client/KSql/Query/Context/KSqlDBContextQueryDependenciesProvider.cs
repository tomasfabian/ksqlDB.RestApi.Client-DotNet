using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

/// <summary>
/// Provider of dependencies needed to handle ksqlDB queries via REST APIs' 'query' endpoint.
/// </summary>
internal class KSqlDBContextQueryDependenciesProvider : KSqlDBContextDependenciesProvider
{
  /// <summary>
  /// Initializes a new instance of <see cref="KSqlDBContextQueryDependenciesProvider"/> with the specified <see cref="KSqlDBContextOptions"/>. 
  /// </summary>
  /// <param name="kSqlDbContextOptions">The options for ksqlDB context.</param>
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
