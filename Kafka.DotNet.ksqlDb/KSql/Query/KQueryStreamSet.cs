using System.Linq.Expressions;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.KSql.Query
{
  internal class KQueryStreamSet<TEntity> : KStreamSet<TEntity>
  {
    public KQueryStreamSet(IServiceScopeFactory serviceScopeFactory, QueryContext queryContext) 
      : base(serviceScopeFactory, queryContext)
    {
    }

    public KQueryStreamSet(IServiceScopeFactory serviceScopeFactory, QueryContext queryContext, Expression expression) 
      : base(serviceScopeFactory, expression, queryContext)
    {
    }
  }
}