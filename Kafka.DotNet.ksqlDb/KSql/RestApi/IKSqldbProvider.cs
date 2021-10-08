using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{
  public interface IKSqlDbProvider
  {
    /// <summary>
    /// Sets Basic HTTP authentication mechanism.
    /// </summary>
    /// <param name="credentials">User credentials.</param>
    void SetCredentials(BasicAuthCredentials credentials);
    IAsyncEnumerable<T> Run<T>(object parameters, CancellationToken cancellationToken = default);
    Task<QueryStream<T>> RunAsync<T>(object parameters, CancellationToken cancellationToken = default);
  }
}