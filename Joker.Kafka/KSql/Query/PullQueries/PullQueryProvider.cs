using System;
using System.Linq.Expressions;
using Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.KSql.Query.PullQueries
{
  internal class PullQueryProvider : IPullQueryProvider
  {
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly QueryContext queryContext;

    public PullQueryProvider(IServiceScopeFactory serviceScopeFactory, QueryContext queryContext = null)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      this.queryContext = queryContext;
    }
    
    IPullable<TResult> IPullQueryProvider.CreateQuery<TResult>(Expression expression)
    {
      return new KPullSet<TResult>(serviceScopeFactory, expression, queryContext);
    }
  }
}