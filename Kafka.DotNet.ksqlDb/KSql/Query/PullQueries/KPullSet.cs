using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.KSql.Query.PullQueries
{
  internal abstract class KPullSet : KSet, IPullable
  {
    public IPullQueryProvider Provider { get; internal set; }
    
    internal QueryContext QueryContext { get; set; }
  }

  internal sealed class KPullSet<TEntity> : KPullSet, IPullable<TEntity>
  {
    private readonly IServiceScopeFactory serviceScopeFactory;

    internal KPullSet(IServiceScopeFactory serviceScopeFactory, QueryContext queryContext = null)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

      QueryContext = queryContext;

      Provider = new PullQueryProvider(serviceScopeFactory, queryContext);

      Expression = Expression.Constant(this);
    }

    internal KPullSet(IServiceScopeFactory serviceScopeFactory, Expression expression, QueryContext queryContext = null)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

      QueryContext = queryContext;

      Provider = new PullQueryProvider(serviceScopeFactory, queryContext);

      Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override Type ElementType => typeof(TEntity);

    /// <summary>
    /// Pulls the first value or returns NULL from the materialized view and terminates. 
    /// </summary>
    public ValueTask<TEntity> GetAsync(CancellationToken cancellationToken = default)
    {
      var dependencies = GetDependencies();

      return dependencies.KsqlDBProvider.Run<TEntity>(dependencies.QueryStreamParameters, cancellationToken)
        .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Pulls all values from the materialized view and terminates. 
    /// </summary>
    public IAsyncEnumerable<TEntity> GetManyAsync(CancellationToken cancellationToken = default)
    {
      var dependencies = GetDependencies();

      return dependencies.KsqlDBProvider.Run<TEntity>(dependencies.QueryStreamParameters, cancellationToken);
    }

    internal IKStreamSetDependencies GetDependencies()
    {
      using var serviceScope = serviceScopeFactory.CreateScope();

      var dependencies = serviceScope.ServiceProvider.GetRequiredService<IKStreamSetDependencies>();

      dependencies.KSqlQueryGenerator.ShouldEmitChanges = false;

      var ksqlQuery = dependencies.KSqlQueryGenerator.BuildKSql(Expression, QueryContext);

      var queryParameters = dependencies.QueryStreamParameters;
      queryParameters.Sql = ksqlQuery;

      return dependencies;
    }
  }
}