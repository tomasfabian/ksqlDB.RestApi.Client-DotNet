using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  public interface IKSqlDBContext : IKSqlDBStatementsContext, IAsyncDisposable
  {
#if !NETSTANDARD
    IQbservable<TEntity> CreateQueryStream<TEntity>(string fromItemName = null);
    IAsyncEnumerable<TEntity> CreateQueryStream<TEntity>(QueryStreamParameters queryStreamParameters, CancellationToken cancellationToken = default);
#endif

    IQbservable<TEntity> CreateQuery<TEntity>(string fromItemName = null);
    IAsyncEnumerable<TEntity> CreateQuery<TEntity>(QueryParameters queryParameters, CancellationToken cancellationToken = default);
    
    IPullable<TEntity> CreatePullQuery<TEntity>(string tableName = null);
    ValueTask<TEntity> ExecutePullQuery<TEntity>(string ksql, CancellationToken cancellationToken = default);
  }
}