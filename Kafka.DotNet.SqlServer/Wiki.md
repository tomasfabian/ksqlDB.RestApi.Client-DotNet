Kafka.DotNet.SqlServer is a client API for consuming row-level table changes (CDC - Change Data Capture) from a Sql Server databases with the Debezium connector streaming platform.

### Blazor Sample 
Set docker-compose.csproj as startup project in Kafka.DotNet.InsideOut.sln.

### Nuget
```
Install-Package Kafka.DotNet.SqlServer -Version 0.1.0-rc.2

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

class Program
{
  static string connectionString = @"Server=127.0.0.1,1433;User Id = SA;Password=<YourNewStrong@Passw0rd>;Initial Catalog = Sensors;MultipleActiveResultSets=true";

  static string bootstrapServers = "localhost:29092";
  static string KsqlDbUrl => @"http:\\localhost:8088";

  static string tableName = "Sensors";
  static string schemaName = "dbo";

  static async Task Main(string[] args)
  {
    await CreateSensorsCdcStreamAsync();

    await TryEnableCdcAsync();

    await CreateConnectorAsync();

    await using var context = new KSqlDBContext(KsqlDbUrl);

    var semaphoreSlim = new SemaphoreSlim(0, 1);

    var cdcSubscription = context.CreateQuery<DatabaseChangeObject<IoTSensor>>("sqlserversensors")
      .WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Take(5)
      .ToObservable()
      .Subscribe(cdc =>
        {
          var operationType = cdc.OperationType;
          Console.WriteLine(operationType);

          switch (operationType)
          {
            case ChangeDataCaptureType.Created:
              Console.WriteLine($"Value: {cdc.EntityAfter.Value}");
              break;
            case ChangeDataCaptureType.Updated:

              Console.WriteLine($"Value before: {cdc.EntityBefore.Value}");
              Console.WriteLine($"Value after: {cdc.EntityAfter.Value}");
              break;
            case ChangeDataCaptureType.Deleted:
              Console.WriteLine($"Value: {cdc.EntityBefore.Value}");
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

    using (cdcSubscription)
    {
    }
  }
}

public record IoTSensor
{
	public string SensorId { get; set; }
	public int Value { get; set; }
}
```

### CdcClient (v0.1.0)
```C#
using Kafka.DotNet.SqlServer.Cdc;

private static async Task TryEnableCdcAsync()
{
  var cdcClient = new CdcClient(connectionString);

  try
  {
    await cdcClient.CdcEnableDbAsync();

    await cdcClient.CdcEnableTableAsync(tableName);
  }
  catch (Exception e)
  {
    Console.WriteLine(e);
  }
}
```

### SqlServerConnectorMetadata, ConnectorMetadata (v0.1.0)

```C#
using System;
using System.Threading.Tasks;
using Kafka.DotNet.SqlServer.Cdc.Connectors;

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

### KsqlDbConnect (v.0.1.0)
```C#
using Kafka.DotNet.SqlServer.Connect;

private static async Task CreateConnectorAsync()
{
  var ksqlDbConnect = new KsqlDbConnect(new Uri(KsqlDbUrl));

  SqlServerConnectorMetadata connectorMetadata = CreateConnectorMetadata();

  await ksqlDbConnect.CreateConnectorAsync(connectorName: "MSSQL_SENSORS_CONNECTOR", connectorMetadata);
}
```

### Creating a stream for CDC - Change data capture  (kafka.dotnet.ksqldb)

```C#
using System;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.SqlServer.Cdc;

private static async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
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

  var httpResponseMessage = await restApiClient.CreateStreamAsync<DatabaseChangeObject>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
    .ConfigureAwait(false);
}
```

### Disable CDC from.NET (v0.1.0)
```C#
var cdcClient = new Kafka.DotNet.SqlServer.Cdc.CdcClient(connectionString);

await cdcClient.CdcDisableTableAsync(tableName, schemaName).ConfigureAwait(false);

await cdcClient.CdcDisableDbAsync().ConfigureAwait(false);
```

### `DatabaseChangeObject<TEntity>` (v.0.1.0)
```C#
using System;
using System.Reactive;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.SqlServer.Cdc;

await using var context = new KSqlDBContext(@"http:\\localhost:8088");

context.CreateQuery<DatabaseChangeObject<IoTSensor>>("sqlserversensors")
  .Subscribe(new AnonymousObserver<DatabaseChangeObject<IoTSensor>>(dco =>
  {
    Console.WriteLine($"Operation type: {dco.OperationType}");
    Console.WriteLine($"Before: {dco.Before}");
    Console.WriteLine($"EntityBefore: {dco.EntityBefore?.Value}");
    Console.WriteLine($"After: {dco.After}");
    Console.WriteLine($"EntityAfter: {dco.EntityAfter?.Value}");
    Console.WriteLine($"Source: {dco.Source}");
  }));
```

### ksqlDB connector info
```KSQL
SHOW CONNECTORS;

DESCRIBE CONNECTOR MSSQL_SENSORS_CONNECTOR;
```

### ksqlDB Cleanup
```KSQL
DROP CONNECTOR MSSQL_SENSORS_CONNECTOR;

DROP STREAM sqlserversensors DELETE TOPIC;
```

# Debezium connector for Sql Server
[Download Debezium connector](https://www.confluent.io/hub/debezium/debezium-connector-sqlserver)

[Deployment](https://debezium.io/documentation/reference/1.5/connectors/sqlserver.html#sqlserver-deploying-a-connector)

### Linqpad
[Kafka.DotNet.SqlServer](https://github.com/tomasfabian/Kafka.DotNet.ksqlDB/blob/main/Samples/Kafka.DotNet.ksqlDB.LinqPad/kafka.dotnet.sqlserver.linq)

### Related sources
[Debezium](https://github.com/debezium/debezium)

[Debezium source connector for Sql server](https://debezium.io/documentation/reference/1.5/connectors/sqlserver.html)

[ksqlDB](https://ksqldb.io/)

# Acknowledgements:
- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/)