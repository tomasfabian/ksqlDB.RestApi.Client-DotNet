using System.Linq.Expressions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.KSql.Query;

internal class QbservableProvider : IKSqlQbservableProvider
{
  private readonly IServiceScopeFactory serviceScopeFactory;
  private readonly QueryContext queryContext;

  public QbservableProvider(IServiceScopeFactory serviceScopeFactory, QueryContext? queryContext = null)
  {
    this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    this.queryContext = queryContext ?? new QueryContext();
  }
    
  IQbservable<TResult> IQbservableProvider.CreateQuery<TResult>(Expression expression)
  {
    return new KQueryStreamSet<TResult>(serviceScopeFactory, queryContext, expression);
  }
}
