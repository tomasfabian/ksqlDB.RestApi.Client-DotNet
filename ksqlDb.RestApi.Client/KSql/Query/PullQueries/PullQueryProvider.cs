using System;
using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query.PullQueries
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