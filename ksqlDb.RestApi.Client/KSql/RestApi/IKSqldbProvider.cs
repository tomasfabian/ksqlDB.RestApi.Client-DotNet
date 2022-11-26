using ksqlDB.RestApi.Client.KSql.RestApi.Query;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

public interface IKSqlDbProvider
{
  IAsyncEnumerable<T> Run<T>(object parameters, CancellationToken cancellationToken = default);
  Task<QueryStream<T>> RunAsync<T>(object parameters, CancellationToken cancellationToken = default);
}