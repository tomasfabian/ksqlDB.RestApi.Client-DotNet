using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Linq.PullQueries;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parameters;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Context
{
  public interface IKSqlDBContext : IKSqlDBStatementsContext, IAsyncDisposable
  {
#if !NETSTANDARD
    IQbservable<TEntity> CreateQueryStream<TEntity>(string streamName = null);
    IAsyncEnumerable<TEntity> CreateQueryStream<TEntity>(QueryStreamParameters queryStreamParameters, CancellationToken cancellationToken = default);
#endif

    IQbservable<TEntity> CreateQuery<TEntity>(string streamName = null);
    IAsyncEnumerable<TEntity> CreateQuery<TEntity>(QueryParameters queryParameters, CancellationToken cancellationToken = default);
    
    IPullable<TEntity> CreatePullQuery<TEntity>(string tableName = null);
    ValueTask<TEntity> ExecutePullQuery<TEntity>(string ksql, CancellationToken cancellationToken = default);
  }
}