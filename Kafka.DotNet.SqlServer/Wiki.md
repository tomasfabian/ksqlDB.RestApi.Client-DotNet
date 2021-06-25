Kafka.DotNet.SqlServer is a client API for consuming row-level table changes (CDC - Change Data Capture) from a Sql Server databases with the Debezium connector streaming platform.

### Blazor Sample 
Set docker-compose.csproj as startup project in Kafka.DotNet.InsideOut.sln.
Work in progress

### Nuget
```
Install-Package Kafka.DotNet.SqlServer -Version 0.1.0-rc.1

Install-Package Kafka.DotNet.ksqlDB
```

### CdcClient (v0.1.0)
```C#
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Generators;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.SqlServer.Cdc;
using Kafka.DotNet.SqlServer.Cdc.Connectors;
using Kafka.DotNet.SqlServer.Cdc.Extensions;

string connectionString = @"Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

string bootstrapServers = "localhost:29092";
string KsqlDbUrl => @"http:\\localhost:8088";
    
string tableName = "Sensors";
string schemaName = "dbo";

ICdcClient CdcProvider { get; set; }

async Task Main()
{
  CdcProvider = new Kafka.DotNet.SqlServer.Cdc.CdcClient(connectionString);

  await CreateSensorsCdcStreamAsync();

  await CdcProvider.CdcEnableDbAsync().ConfigureAwait(false);

  await CdcProvider.CdcEnableTable(tableName, schemaName).ConfigureAwait(false);

  await CreateConnectorAsync(tableName);

  await using var context = new KSqlDBContext(KsqlDbUrl);

  var semaphoreSlim = new SemaphoreSlim(0, 1);
	
  var cdcSubscription = context.CreateQuery<DatabaseChangeObject>("sqlserversensors")
    .WithOffsetResetPolicy(AutoOffsetReset.Latest)
    .Take(5)
    .ToObservable()
    .Subscribe(cdc =>
      {
        var operationType = cdc.Op.ToChangeDataCaptureType();
        Console.WriteLine(operationType);

        switch (operationType)
        {
          case ChangeDataCaptureType.Updated:

            var sensorBefore = System.Text.Json.JsonSerializer.Deserialize<IoTSensor>(cdc.Before);
            var sensorAfter = System.Text.Json.JsonSerializer.Deserialize<IoTSensor>(cdc.After);
            break;
        }
      }, onError: error =>
      {
        semaphoreSlim.Release();
            
        Console.WriteLine($"Exception: {error.Message}");
      },
      onCompleted: () =>
      {
        semaphoreSlim.Release();
        Console.WriteLine("Completed");
      });


  await semaphoreSlim.WaitAsync();
	
  using(cdcSubscription){
  }
}

public record IoTSensor
{
	public string SensorId { get; set; }
	public int Value { get; set; }
}
```

### ConnectorMetadata (v0.1.0)

```C#
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.SqlServer.Cdc.Connectors;

private async Task CreateConnectorAsync(string tableName, string schemaName = "dbo")
{
  var createConnectorStatement = CreateConnector();

  KSqlDbStatement ksqlDbStatement = new(createConnectorStatement);

  var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement).ConfigureAwait(false);
}

private string CreateConnector()
{
  var createConnector = new ConnectorMetadata(connectionString)
    .SetJsonKeyConverter()
    .SetJsonValueConverter()
    .SetTableIncludeListPropertyName($"{schemaName}.{tableName}")
    .SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
    .SetProperty("database.history.kafka.topic", $"dbhistory.{tableName}")
    .SetProperty("database.server.name", "sqlserver2019")
    .SetProperty("key.converter.schemas.enable", "false")
    .SetProperty("value.converter.schemas.enable", "false")
    .SetProperty("include.schema.changes", "false")
    .ToStatement(connectorName: "MSSQL_SENSORS_CONNECTOR");
      
  return createConnector;
}

private Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
{
  var httpClientFactory = new HttpClientFactory(new Uri(KsqlDbUrl));

  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  return restApiClient.ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
}
```

Above mentioned C# snippet is equivavelent to:
```KSQL
CREATE SOURCE CONNECTOR MSSQL_SENSORS_CONNECTOR WITH (
  'connector.class'= 'io.debezium.connector.sqlserver.SqlServerConnector', 
  'database.port'= '1433', 
  'database.hostname'= '127.0.0.1', 
  'database.user'= 'SA', 
  'database.password'= '<YourNewStrong@Passw0rd>', 
  'database.dbname'= 'Sensors', 
  'key.converter'= 'org.apache.kafka.connect.json.JsonConverter', 
  'value.converter'= 'org.apache.kafka.connect.json.JsonConverter', 
  'table.include.list'= 'dbo.Sensors', 
  'database.history.kafka.bootstrap.servers'= 'localhost:29092', 
  'database.history.kafka.topic'= 'dbhistory.Sensors', 
  'database.server.name'= 'sqlserver2019', 
  'key.converter.schemas.enable'= 'false', 
  'value.converter.schemas.enable'= 'false', 
  'include.schema.changes'= 'false'
);
```

### Creating a stream for CDC - Change data capture  (kafka.dotnet.ksqldb)

```C#
private async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
{
  await using var context = new KSqlDBContext(KsqlDbUrl);

  string fromName = "sqlserversensors";
  string kafkaTopic = "sqlserver2019.dbo.Sensors";

  var httpClientFactory = new HttpClientFactory(new Uri(KsqlDbUrl));

  var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

  EntityCreationMetadata metadata = new()
  {
    EntityName = fromName,
    KafkaTopic = kafkaTopic,
    ValueFormat = SerializationFormats.Json,
    Partitions = 1,
    Replicas = 1
  };

  var ksql = StatementGenerator.CreateStream<DatabaseChangeObject>(metadata, ifNotExists: true);

  var httpResponseMessage = await restApiClient.CreateStreamAsync<DatabaseChangeObject>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
    .ConfigureAwait(false);
}
```

### Disable CDC from.NET 
```C#
var cdcProvider = new Kafka.DotNet.SqlServer.Cdc.CdcClient(connectionString);

await cdcProvider.CdcDisableTableAsync(tableName, schemaName).ConfigureAwait(false);

await cdcProvider.CdcDisableDbAsync().ConfigureAwait(false);
```
or 
```C#
await cdcProvider.DisableAsync(tableName, schemaName).ConfigureAwait(false);
```

### ksqlDB Cleanup
```KSQL
show connectors;

drop connector MSSQL_SENSORS_CONNECTOR;

drop stream sqlserversensors delete topic;
```

### Linqpad

### Related sources
[Debezium](https://github.com/debezium/debezium)

[Debezium connector for Sql server](https://debezium.io/documentation/reference/connectors/sqlserver.html)

[ksqlDB](https://ksqldb.io/)

# Acknowledgements:
- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/)