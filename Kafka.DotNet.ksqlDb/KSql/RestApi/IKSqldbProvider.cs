using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{
  public interface IKSqlDbProvider
  {
    IAsyncEnumerable<T> Run<T>(object parameters, CancellationToken cancellationToken = default);
    Task<Query<T>> RunAsync<T>(object parameters, CancellationToken cancellationToken = default);
  }
}