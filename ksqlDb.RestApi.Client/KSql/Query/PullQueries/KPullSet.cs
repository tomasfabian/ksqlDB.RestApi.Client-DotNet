using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query.PullQueries;

internal abstract class KPullSet : KSet, IPullable
{
  public IPullQueryProvider Provider { get; internal set; } = null!;

  internal QueryContext QueryContext { get; set; } = null!;
}

internal sealed class KPullSet<TEntity> : KPullSet, IPullable<TEntity>
{
  private readonly IServiceScopeFactory serviceScopeFactory;

  internal KPullSet(IServiceScopeFactory serviceScopeFactory, QueryContext? queryContext = null)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    QueryContext = queryContext ?? new QueryContext();

    Provider = new PullQueryProvider(serviceScopeFactory, QueryContext);

    Expression = Expression.Constant(this);
  }

  internal KPullSet(IServiceScopeFactory serviceScopeFactory, Expression expression, QueryContext? queryContext = null)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    QueryContext = queryContext ?? new QueryContext();

    Provider = new PullQueryProvider(serviceScopeFactory, QueryContext);

    Expression = expression ?? throw new ArgumentNullException(nameof(expression));
  }

  public override Type ElementType => typeof(TEntity);

  /// <summary>
  /// Pulls the first value or returns NULL from the materialized view and terminates. 
  /// </summary>
  public ValueTask<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
  {
    var dependencies = GetDependencies();

    return dependencies.KSqlDbProvider.Run<TEntity>(GetQueryStreamParameters(dependencies), cancellationToken)
      .FirstOrDefaultAsync(cancellationToken);
  }

  /// <summary>
  /// Pulls all values from the materialized view asynchronously and terminates. 
  /// </summary>
  public IAsyncEnumerable<TEntity> GetManyAsync(CancellationToken cancellationToken = default)
  {
    var dependencies = GetDependencies();

    return dependencies.KSqlDbProvider.Run<TEntity>(GetQueryStreamParameters(dependencies), cancellationToken);
  }

  internal IKPullSetDependencies GetDependencies()
  {
    using var serviceScope = serviceScopeFactory.CreateScope();

    var dependencies = serviceScope.ServiceProvider.GetRequiredService<IKPullSetDependencies>();

    dependencies.KSqlQueryGenerator.ShouldEmitChanges = false;

    return dependencies;
  }

  internal IKSqlDbParameters GetQueryStreamParameters(IKPullSetDependencies dependencies)
  {
    var ksqlQuery = dependencies.KSqlQueryGenerator.BuildKSql(Expression, QueryContext);

    var queryParameters = dependencies.QueryStreamParameters;
    queryParameters.Sql = ksqlQuery;

    return queryParameters;
  }
}
