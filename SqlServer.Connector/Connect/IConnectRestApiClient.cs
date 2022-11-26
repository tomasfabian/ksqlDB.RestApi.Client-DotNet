using SqlServer.Connector.Cdc.Connectors;

namespace SqlServer.Connector.Connect
{
  public interface IConnectRestApiClient
  {
    Task<HttpResponseMessage> PostConnectorAsync(ConnectorMetadata connectorMetadata, string connectorName, CancellationToken cancellationToken = default);
  }
}
