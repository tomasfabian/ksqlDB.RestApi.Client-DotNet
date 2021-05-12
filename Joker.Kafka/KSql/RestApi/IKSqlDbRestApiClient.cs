using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{
  public interface IKSqlDbRestApiClient
  {
    Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlStatement, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateTable<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateStream<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateOrReplaceTable<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateOrReplaceStream<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default);
    /// <summary>
    /// Produce a row into an existing stream or table and its underlying topic based on explicitly specified values.
    /// </summary>
    Task<HttpResponseMessage> InsertIntoAsync<T>(T entity, InsertProperties insertProperties = null, CancellationToken cancellationToken = default);
  }
}