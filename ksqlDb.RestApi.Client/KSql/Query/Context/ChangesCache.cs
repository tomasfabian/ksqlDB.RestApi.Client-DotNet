using System.Collections.Concurrent;
using System.Text;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

internal sealed class ChangesCache : ConcurrentQueue<KSqlDbStatement>
{
  private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

  internal async Task<HttpResponseMessage> SaveChangesAsync(IKSqlDbRestApiClient restApiClient, CancellationToken cancellationToken)
  {
    await semaphoreSlim.WaitAsync(cancellationToken);

    try
    {
      return await SaveChangesIntAsync(restApiClient, cancellationToken);
    }
    finally
    {
      semaphoreSlim.Release();
    }
  }

  internal Task<HttpResponseMessage> SaveChangesIntAsync(IKSqlDbRestApiClient restApiClient, CancellationToken cancellationToken)
  {
    var stringBuilder = new StringBuilder();

    while (!IsEmpty)
    {
      TryDequeue(out var statement);

      stringBuilder.AppendLine(statement.Sql);
    }

    var ksqlDbStatement = new KSqlDbStatement(stringBuilder.ToString());

    return restApiClient.ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
  }
}