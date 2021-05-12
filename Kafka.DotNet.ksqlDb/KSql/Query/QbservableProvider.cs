using System;
using System.Linq.Expressions;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal class QbservableProvider : IKSqlQbservableProvider
  {
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly QueryContext queryContext;

    public QbservableProvider(IServiceScopeFactory serviceScopeFactory, QueryContext queryContext = null)
    {
      this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      this.queryContext = queryContext;
    }
    
    IQbservable<TResult> IQbservableProvider.CreateQuery<TResult>(Expression expression)
    {
      return new KQueryStreamSet<TResult>(serviceScopeFactory, queryContext, expression);
    }
  }
}