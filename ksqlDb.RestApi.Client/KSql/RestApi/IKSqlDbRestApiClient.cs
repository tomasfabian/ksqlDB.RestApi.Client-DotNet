using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ksqlDb.RestApi.Client.KSql.RestApi.Responses.Asserts;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Connectors;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Streams;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Topics;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

public interface IKSqlDbRestApiClient
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
  /// Generates raw string Insert Into, but does not execute it.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="entity">Entity for insertion.</param>
  /// <param name="insertProperties">Overrides conventions.</param>
  /// <returns></returns>
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
  /// Terminate a persistent query. Persistent queries run continuously until they are explicitly terminated.
  /// </summary>
  /// <param name="queryId">Id of the query to terminate.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  Task<StatementResponse[]> TerminatePersistentQueryAsync(string queryId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Terminate a push query.
  /// </summary>
  /// <param name="queryId">Id of the query to terminate.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  Task<HttpResponseMessage> TerminatePushQueryAsync(string queryId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Drops an existing stream.
  /// DROP STREAM [IF EXISTS] stream_name [DELETE TOPIC];
  /// </summary>
  /// <param name="streamName">Name of the stream to delete.</param>
  /// <param name="useIfExistsClause">If the IF EXISTS clause is present, the statement doesn't fail if the stream doesn't exist.</param>
  /// <param name="deleteTopic">If the DELETE TOPIC clause is present, the stream's source topic is marked for deletion.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  Task<HttpResponseMessage> DropStreamAsync(string streamName, bool useIfExistsClause, bool deleteTopic, CancellationToken cancellationToken = default);

  /// <summary>
  /// Drops an existing stream.
  /// DROP STREAM stream_name;
  /// </summary>
  /// <param name="streamName">Name of the stream to delete.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  Task<HttpResponseMessage> DropStreamAsync(string streamName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Drops an existing table.
  /// DROP TABLE [IF EXISTS] table_name [DELETE TOPIC];
  /// </summary>
  /// <param name="tableName">Name of the table to delete.</param>
  /// <param name="useIfExistsClause">If the IF EXISTS clause is present, the statement doesn't fail if the table doesn't exist.</param>
  /// <param name="deleteTopic">If the DELETE TOPIC clause is present, the table's source topic is marked for deletion.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  Task<HttpResponseMessage> DropTableAsync(string tableName, bool useIfExistsClause, bool deleteTopic, CancellationToken cancellationToken = default);

  /// <summary>
  /// Drops an existing table.
  /// DROP TABLE table_name;
  /// </summary>
  /// <param name="tableName">Name of the table to delete.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns></returns>
  Task<HttpResponseMessage> DropTableAsync(string tableName, CancellationToken cancellationToken = default);

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
  /// <param name="typeName">Optional name of the type. Otherwise the type name is inferred from the generic type name.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> CreateTypeAsync<T>(string typeName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes a type alias from ksqlDB. This statement doesn't fail if the type is in use in active queries or user-defined functions.
  /// </summary>
  /// <param name="typeName">Name of the type to remove.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> DropTypeAsync(string typeName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes a type alias from ksqlDB. This statement doesn't fail if the type is in use in active queries or user-defined functions. The statement doesn't fail if the type doesn't exist.
  /// </summary>
  /// <param name="typeName">Name of the type to remove.</param>
  /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
  /// <returns>Http response object.</returns>
  Task<HttpResponseMessage> DropTypeIfExistsAsync(string typeName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Asserts that a topic exists or does not exist.
  /// </summary>
  /// <param name="options">The assert topic options such as topic name and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert topic responses. If the assertion fails, then an error will be returned.</returns>
  Task<AssertTopicResponse[]> AssertTopicExistsAsync(AssertTopicOptions options, CancellationToken cancellationToken = default);

  /// <summary>
  /// Asserts that a topic exists or does not exist.
  /// </summary>
  /// <param name="options">The assert topic options such as topic name and timeout.</param>
  /// <param name="cancellationToken"></param>
  /// <returns>Assert topic responses. If the assertion fails, then an error will be returned.</returns>
  Task<AssertTopicResponse[]> AssertTopicNotExistsAsync(AssertTopicOptions options, CancellationToken cancellationToken = default);
}