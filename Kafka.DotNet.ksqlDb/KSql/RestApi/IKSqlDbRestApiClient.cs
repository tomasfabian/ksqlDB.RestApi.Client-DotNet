using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Connectors;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Queries;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Streams;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Tables;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Topics;
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

    /// <summary>
    /// Create a new source connector in the Kafka Connect cluster with the configuration passed in the config parameter.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="connectorName">Name of the connector to create.</param>
    /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement does not fail if a connector with the supplied name already exists.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> CreateSourceConnectorAsync(IDictionary<string, string> config, string connectorName, bool ifNotExists = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new sink connector in the Kafka Connect cluster with the configuration passed in the config parameter.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="connectorName">Name of the connector to create.</param>
    /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement does not fail if a connector with the supplied name already exists.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> CreateSinkConnectorAsync(IDictionary<string, string> config, string connectorName, bool ifNotExists = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of topics.</returns>
    Task<TopicsResponse[]> GetTopicsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to, including hidden topics.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of topics.</returns>
    Task<TopicsResponse[]> GetAllTopicsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of topics. Also displays consumer groups and their active consumer counts.</returns>
    Task<TopicsExtendedResponse[]> GetTopicsExtendedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the available topics in the Kafka cluster that ksqlDB is configured to connect to, including hidden topics.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of topics. Also displays consumer groups and their active consumer counts.</returns>
    Task<TopicsExtendedResponse[]> GetAllTopicsExtendedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists queries running in the cluster.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of queries.</returns>
    Task<QueriesResponse[]> GetQueriesAsync(CancellationToken cancellationToken = default);
  }
}