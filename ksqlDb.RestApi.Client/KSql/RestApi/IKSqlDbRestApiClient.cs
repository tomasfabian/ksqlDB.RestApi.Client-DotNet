using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Connectors;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Streams;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

public interface IKSqlDbRestApiClient : IKSqlDbAssertionsRestApiClient, IKSqlDbDropRestApiClient
{
  /// <summary>
  /// Sets Basic HTTP authentication mechanism.
  /// </summary>
  /// <param name="credentials">User credentials.</param>
  /// <returns>This instance.</returns>
  IKSqlDbRestApiClient SetCredentials(BasicAuthCredentials credentials);

  /// <summary>
  /// Run a sequence of SQL statements.
  /// </summary>
  /// <param name="ksqlDbStatement">The text of the SQL statements.</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a new stream with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T">The type that represents the stream.</typeparam>
  /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a stream with the same name already exists.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateStreamAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a new stream or replace an existing one with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T">The type that represents the stream.</typeparam>
  /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Http response object.</returns>
  ///
  Task<HttpResponseMessage> CreateOrReplaceStreamAsync<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default);


  /// <summary>
  /// Create a new read-only source stream with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T">The type that represents the stream.</typeparam>
  /// <param name="creationMetadata">Stream properties, specify details about your stream by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a stream with the same name already exists.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateSourceStreamAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a new table with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="creationMetadata">Table properties, specify details about your table by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a table with the same name already exists.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateTableAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a new read-only table with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="creationMetadata">Table properties, specify details about your table by using the WITH clause.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement won't fail if a table with the same name already exists.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateSourceTableAsync<T>(EntityCreationMetadata creationMetadata, bool ifNotExists = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a new table or replace an existing one with the specified columns and properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="creationMetadata">Table properties, specify details about your table by using the WITH clause.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateOrReplaceTableAsync<T>(EntityCreationMetadata creationMetadata, CancellationToken cancellationToken = default);

  /// <summary>
  /// Produce a row into an existing stream or table and its underlying topic based on explicitly specified entity properties.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="entity">Entity for insertion.</param>
  /// <param name="insertProperties">Overrides conventions.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> InsertIntoAsync<T>(T entity, InsertProperties insertProperties = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Generates raw 'Insert Into' string, but does not execute it.
  /// </summary>
  /// <typeparam name="T">Type of entity</typeparam>
  /// <param name="insertValues">Insert values</param>
  /// <param name="insertProperties">Insert configuration</param>
  /// <returns>A <see cref="KSqlDbStatement"/></returns>
  KSqlDbStatement ToInsertStatement<T>(InsertValues<T> insertValues, InsertProperties insertProperties = null);

  /// <summary>
  /// Generates raw 'Insert Into' string, but does not execute it.
  /// </summary>
  /// <typeparam name="T">Type of entity</typeparam>
  /// <param name="entity">Entity for insertion.</param>
  /// <param name="insertProperties">Overrides conventions.</param>
  /// <returns>A <see cref="KSqlDbStatement"/></returns>
  KSqlDbStatement ToInsertStatement<T>(T entity, InsertProperties insertProperties = null);

  /// <summary>
  /// List the defined streams.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<StreamsResponse[]> GetStreamsAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// List the defined tables.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<TablesResponse[]> GetTablesAsync(CancellationToken cancellationToken = default);

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
  /// <returns>List of all topics.</returns>
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
  /// <returns>List of all topics. Also displays consumer groups and their active consumer counts.</returns>
  Task<TopicsExtendedResponse[]> GetAllTopicsExtendedAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Lists queries running in the cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns>List of queries.</returns>
  Task<QueriesResponse[]> GetQueriesAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// List all connectors in the Connect cluster.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<ConnectorsResponse[]> GetConnectorsAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a new source connector in the Kafka Connect cluster with the configuration passed in the config parameter.
  /// </summary>
  /// <param name="config">Configuration passed into the WITH clause.</param>
  /// <param name="connectorName">Name of the connector to create.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement does not fail if a connector with the supplied name already exists.</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<HttpResponseMessage> CreateSourceConnectorAsync(IDictionary<string, string> config, string connectorName, bool ifNotExists = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create a new sink connector in the Kafka Connect cluster with the configuration passed in the config parameter.
  /// </summary>
  /// <param name="config">Configuration passed into the WITH clause.</param>
  /// <param name="connectorName">Name of the connector to create.</param>
  /// <param name="ifNotExists">If the IF NOT EXISTS clause is present, the statement does not fail if a connector with the supplied name already exists.</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<HttpResponseMessage> CreateSinkConnectorAsync(IDictionary<string, string> config, string connectorName, bool ifNotExists = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Pause a persistent query.
  /// </summary>
  /// <param name="queryId">ID of the query to pause.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  Task<StatementResponse[]> PausePersistentQueryAsync(string queryId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Resume a paused persistent query.
  /// </summary>
  /// <param name="queryId">ID of the query to resume.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  Task<StatementResponse[]> ResumePersistentQueryAsync(string queryId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Terminate a persistent query. Persistent queries run continuously until they are explicitly terminated.
  /// </summary>
  /// <param name="queryId">ID of the query to terminate.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  Task<StatementResponse[]> TerminatePersistentQueryAsync(string queryId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Terminate a push query.
  /// </summary>
  /// <param name="queryId">ID of the query to terminate.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Statement response</returns>
  Task<HttpResponseMessage> TerminatePushQueryAsync(string queryId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create an alias for a complex type declaration.
  /// The CREATE TYPE statement registers a type alias directly in KSQL. Any types registered by using this command can be leveraged in future statements. The CREATE TYPE statement works in interactive and headless modes.
  /// Any attempt to register the same type twice, without a corresponding DROP TYPE statement, will fail.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateTypeAsync<T>(CancellationToken cancellationToken = default);

  /// <summary>
  /// Create an alias for a complex type declaration.
  /// The CREATE TYPE statement registers a type alias directly in KSQL. Any types registered by using this command can be leveraged in future statements. The CREATE TYPE statement works in interactive and headless modes.
  /// Any attempt to register the same type twice, without a corresponding DROP TYPE statement, will fail.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="typeName">Optional name of the type. Otherwise, the type name is inferred from the generic type name.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateTypeAsync<T>(string typeName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Create an alias for a complex type declaration.
  /// The CREATE TYPE statement registers a type alias directly in KSQL. Any types registered by using this command can be leveraged in future statements. The CREATE TYPE statement works in interactive and headless modes.
  /// Any attempt to register the same type twice, without a corresponding DROP TYPE statement, will fail.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="properties">Type configuration</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateTypeAsync<T>(TypeProperties properties, CancellationToken cancellationToken = default);
}
