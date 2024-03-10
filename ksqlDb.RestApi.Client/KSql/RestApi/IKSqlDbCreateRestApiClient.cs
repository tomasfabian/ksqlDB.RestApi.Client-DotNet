using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi
{
  public interface IKSqlDbCreateRestApiClient
  {
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
}
