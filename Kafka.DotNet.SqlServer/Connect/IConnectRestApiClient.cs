using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.SqlServer.Cdc.Connectors;

namespace Kafka.DotNet.SqlServer.Connect
{
  public interface IConnectRestApiClient
  {
    Task<HttpResponseMessage> PostConnectorAsync(ConnectorMetadata connectorMetadata, string connectorName, CancellationToken cancellationToken = default);
  }
}