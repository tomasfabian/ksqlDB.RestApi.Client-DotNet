using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.SqlServer.Cdc.Connectors;

namespace Kafka.DotNet.SqlServer.Connect
{
  public class KsqlDbConnect : IKsqlDbConnect
  {
    private readonly Uri ksqlDbUrl;

    public KsqlDbConnect(Uri ksqlDbUrl)
    {
      this.ksqlDbUrl = ksqlDbUrl ?? throw new ArgumentNullException(nameof(ksqlDbUrl));
    }

    public async Task<HttpResponseMessage> CreateConnectorAsync(string connectorName, SqlServerConnectorMetadata connectorMetadata, CancellationToken cancellationToken = default)
    {
      var createConnector = connectorMetadata.ToStatement(connectorName);

      KSqlDbStatement ksqlDbStatement = new(createConnector);

      var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement, cancellationToken).ConfigureAwait(false);

      return httpResponseMessage;
    }

    private Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
    {
      var httpClientFactory = new HttpClientFactory(ksqlDbUrl);

      var restApiClient = new KSqlDbRestApiClient(httpClientFactory);
      
      return restApiClient.ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
    }
  }
}