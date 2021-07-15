using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Connectors;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Streams;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Tables;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{
  public interface IKSqlDbRestApiClient
  {
    Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlStatement, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateTableAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateStreamAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateOrReplaceTableAsync<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> CreateOrReplaceStreamAsync<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default);
    /// <summary>
    /// Produce a row into an existing stream or table and its underlying topic based on explicitly specified values.
    /// </summary>
    Task<HttpResponseMessage> InsertIntoAsync<T>(T entity, InsertProperties insertProperties = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all connectors in the Connect cluster.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ConnectorsResponse[]> GetConnectorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement doesn't fail if the connector doesn't exist.
    /// </summary>
    /// <param name="connectorName">Name of the connector to drop.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> DropConnectorIfExistsAsync(string connectorName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement fails if the connector doesn't exist.
    /// </summary>
    /// <param name="connectorName">Name of the connector to drop.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> DropConnectorAsync(string connectorName, CancellationToken cancellationToken = default);

    /// <summary>
    /// List the defined tables.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TablesResponse[]> GetTablesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// List the defined streams.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StreamsResponse[]> GetStreamsAsync(CancellationToken cancellationToken = default);
  }
}