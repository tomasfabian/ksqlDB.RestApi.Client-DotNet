using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using SqlServer.Connector.Cdc.Connectors;

namespace SqlServer.Connector.Connect
{
  /// <summary>
  /// REST API client for managing connectors.
  /// </summary>
  public class ConnectRestApiClient : IConnectRestApiClient
  {
    private readonly IHttpClientFactory httpClientFactory;

    public ConnectRestApiClient(IHttpClientFactory httpClientFactory)
    {
      this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    internal static readonly string MediaType = "application/json";

    /// <summary>
    /// Create a new connector.
    /// </summary>
    /// <param name="connectorMetadata">Configuration parameters for the connector.</param>
    /// <param name="connectorName">Name of the connector to create</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> PostConnectorAsync(ConnectorMetadata connectorMetadata, string connectorName, CancellationToken cancellationToken = default)
    {
      if (connectorMetadata == null) throw new ArgumentNullException(nameof(connectorMetadata));
      if (string.IsNullOrWhiteSpace(connectorName))
        throw new ArgumentException("Cannot be null, empty, or contain only whitespace.", nameof(connectorName));

      using var httpClient = httpClientFactory.CreateClient();

      var connector = new Connector
      {
        Name = connectorName,
        Config = connectorMetadata.Properties
      };

      var httpRequestMessage = CreateHttpRequestMessage(connector, HttpMethod.Post, @"/connectors");

      httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue(MediaType));

      var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken)
        .ConfigureAwait(false);

      return httpResponseMessage;
    }

    internal HttpRequestMessage CreateHttpRequestMessage(Connector connector, HttpMethod httpMethod, string endpoint)
    {
      var content = CreateContent(connector);

      var httpRequestMessage = new HttpRequestMessage(httpMethod, endpoint)
      {
        Content = content
      };

      return httpRequestMessage;
    }

    internal StringContent CreateContent(Connector connector)
    {
      var json = JsonSerializer.Serialize(connector);

      var data = new StringContent(json, Encoding.UTF8, MediaType);

      return data;
    }
  }
}