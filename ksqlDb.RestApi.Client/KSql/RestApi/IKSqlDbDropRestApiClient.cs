using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi
{
  public interface IKSqlDbDropRestApiClient
  {
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
    /// Drops an existing table.
    /// DROP TABLE [IF EXISTS] table_name [DELETE TOPIC];
    /// </summary>
    /// <typeparam name="T">The type that represents the table.</typeparam>
    /// <param name="dropFromItemProperties">Configuration for dropping the table.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns></returns>
    Task<HttpResponseMessage> DropTableAsync<T>(DropFromItemProperties dropFromItemProperties, CancellationToken cancellationToken = default);

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
    /// Removes a type alias from ksqlDB. This statement doesn't fail if the type is in use in active queries or user-defined functions.
    /// </summary>
    /// <param name="dropTypeProperties">Configuration for dropping the type.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns>Http response object.</returns>
    Task<HttpResponseMessage> DropTypeAsync<T>(DropTypeProperties dropTypeProperties, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops an existing stream.
    /// DROP STREAM [IF EXISTS] stream_name [DELETE TOPIC];
    /// </summary>
    /// <param name="dropFromItemProperties">Configuration for dropping the stream.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
    /// <returns></returns>
    Task<HttpResponseMessage> DropStreamAsync<T>(DropFromItemProperties dropFromItemProperties, CancellationToken cancellationToken = default);

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
  }
}
