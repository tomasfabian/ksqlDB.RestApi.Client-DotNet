using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.SqlServer.Cdc.Connectors;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace Kafka.DotNet.SqlServer.Connect
{
  /// <summary>
  /// KsqlDbConnect executes Kafka Connect related statements against ksqlDb REST API.
  /// </summary>
  public class KsqlDbConnect : IKsqlDbConnect
  {
    private readonly Uri ksqlDbUrl;

    public KsqlDbConnect(Uri ksqlDbUrl)
    {
      this.ksqlDbUrl = ksqlDbUrl ?? throw new ArgumentNullException(nameof(ksqlDbUrl));
    }

    /// <summary>
    /// Create a new connector in the Kafka Connect cluster with the configuration passed in the connectorMetadata parameter.
    /// </summary>
    /// <param name="connectorName">Name of the connector.</param>
    /// <param name="connectorMetadata">Configuration passed in the WITH clause.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> CreateConnectorAsync(string connectorName, SqlServerConnectorMetadata connectorMetadata, CancellationToken cancellationToken = default)
    {
      var createConnector = connectorMetadata.ToCreateConnectorStatement(connectorName);

      KSqlDbStatement ksqlDbStatement = new(createConnector);

      var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

      return httpResponseMessage;
    }

    /// <summary>
    /// Create a new connector in the Kafka Connect cluster with the configuration passed in the connectorMetadata parameter. The statement does not fail if a connector with the supplied name already exists.
    /// </summary>
    /// <param name="connectorName">Name of the connector.</param>
    /// <param name="connectorMetadata">Configuration passed in the WITH clause.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> CreateConnectorIfNotExistsAsync(string connectorName, SqlServerConnectorMetadata connectorMetadata, CancellationToken cancellationToken = default)
    {
      var createConnector = connectorMetadata.ToCreateConnectorStatement(connectorName, ifNotExists: true);

      KSqlDbStatement ksqlDbStatement = new(createConnector);

      var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

      return httpResponseMessage;
    }

    /// <summary>
    /// Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement doesn't fail if the connector doesn't exist.
    /// </summary>
    /// <param name="connectorName">Name of the connector to drop.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HttpResponseMessage> DropConnectorIfExistsAsync(string connectorName, CancellationToken cancellationToken = default)
    {
      string dropIfExistsStatement = $"DROP CONNECTOR IF EXISTS {connectorName};";

      return ExecuteStatementAsync(dropIfExistsStatement, cancellationToken);
    }

    /// <summary>
    /// Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement fails if the connector doesn't exist.
    /// </summary>
    /// <param name="connectorName">Name of the connector to drop.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HttpResponseMessage> DropConnectorAsync(string connectorName, CancellationToken cancellationToken = default)
    {
      string dropStatement = $"DROP CONNECTOR {connectorName};";

      return ExecuteStatementAsync(dropStatement, cancellationToken);
    }

    /// <summary>
    /// List all connectors in the Connect cluster.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HttpResponseMessage> GetConnectorsAsync(CancellationToken cancellationToken = default)
    {
      string showStatement = "SHOW CONNECTORS;";

      return ExecuteStatementAsync(showStatement, cancellationToken);
    }

    private Task<HttpResponseMessage> ExecuteStatementAsync(string ksqlStatement, CancellationToken cancellationToken = default)
    {
      KSqlDbStatement ksqlDbStatement = new(ksqlStatement);
      
      return ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
    }

    private Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
    {
      var httpClientFactory = new HttpClientFactory(ksqlDbUrl);

      var restApiClient = new KSqlDbRestApiClient(httpClientFactory);
      
      return restApiClient.ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
    }
  }
}