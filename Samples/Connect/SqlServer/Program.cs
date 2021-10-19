using System;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using SqlServer.Connector.Cdc.Connectors;
using SqlServer.Connector.Connect;

namespace Connect.SqlServer
{
  public static class Program
  {
    static string ConnectUrl => @"http://connect:8083";

    static async Task Main(string[] args)
    {
      SqlServerConnectorMetadata connectorMetadata = CreateConnectorMetadata();

      string connectorName = "MSSQL_CDC_CONNECTOR";
	
      var httpClientFactory = new HttpClientFactory(new Uri(ConnectUrl));
	
      var connectRestApiClient = new ConnectRestApiClient(httpClientFactory);
	
      var httpResponseMessage = await connectRestApiClient.PostConnectorAsync(connectorMetadata, connectorName);
    }

    static string connectionString = @"Server=sqlserver2019,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

    static string bootstrapServers = "localhost:29092";

    static string tableName = "Sensors";
    static string schemaName = "dbo";
    private static SqlServerConnectorMetadata CreateConnectorMetadata()
    {
      var createConnector = new SqlServerConnectorMetadata(connectionString)
        .SetTableIncludeListPropertyName($"{schemaName}.{tableName}")
        .SetJsonKeyConverter()
        .SetJsonValueConverter()
        .SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
        .SetProperty("database.history.kafka.topic", $"dbhistory.{tableName}")
        .SetProperty("database.server.name", "sqlserver2019")
        .SetProperty("key.converter.schemas.enable", "false")
        .SetProperty("value.converter.schemas.enable", "false")
        .SetProperty("include.schema.changes", "false");

      return createConnector as SqlServerConnectorMetadata;
    }
  }
}