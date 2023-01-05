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

      var httpClient = new HttpClient()
      {
        BaseAddress = new Uri(ConnectUrl)
      };

      var httpClientFactory = new HttpClientFactory(httpClient);
	
      var connectRestApiClient = new ConnectRestApiClient(httpClientFactory);
	
      var httpResponseMessage = await connectRestApiClient.PostConnectorAsync(connectorMetadata, connectorName);

      var message = await httpResponseMessage.Content.ReadAsStringAsync();

      Console.WriteLine(message);
    }

    static readonly string ConnectionString = @"Server=sqlserver2019,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";
    static readonly string BootstrapServers = "localhost:29092";
    static readonly string TableName = "Sensors";
    static readonly string SchemaName = "dbo";

    private static SqlServerConnectorMetadata CreateConnectorMetadata()
    {
      var createConnector = new SqlServerConnectorMetadata(ConnectionString)
        .SetTableIncludeListPropertyName($"{SchemaName}.{TableName}")
        .SetJsonKeyConverter()
        .SetJsonValueConverter()
        .SetProperty("database.history.kafka.bootstrap.servers", BootstrapServers)
        .SetProperty("database.history.kafka.topic", $"dbhistory.{TableName}")
        .SetProperty("database.server.name", "sqlserver2019")
        .SetProperty("key.converter.schemas.enable", "false")
        .SetProperty("value.converter.schemas.enable", "false")
        .SetProperty("include.schema.changes", "false");

      return (SqlServerConnectorMetadata)createConnector;
    }
  }
}
