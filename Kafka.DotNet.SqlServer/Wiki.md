Kafka.DotNet.SqlServer is a client API for consuming row-level table changes (CDC - [Change Data Capture](https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server?view=sql-server-ver15)) from a Sql Server databases with the Debezium connector streaming platform.

### Blazor Sample 
Set docker-compose.csproj as startup project in Kafka.DotNet.InsideOut.sln.

### Nuget
```
Install-Package Kafka.DotNet.SqlServer -Version 0.1.0-rc.2

Install-Package Kafka.DotNet.ksqlDB
```

### Subscribing to a CDC stream (v0.1.0)
The Debezium connector produces change events from row-level table changes into a kafka topic. The following program shows how to subscribe to such streams with ksqldb
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

    var cdcSubscription = context.CreateQuery<RawDatabaseChangeObject<IoTSensor>>("sqlserversensors")
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

Navigate to http://localhost:9000/topic/sqlserver2019.dbo.Sensors for information about the created kafka topic and view messages with [Kafdrop](https://github.com/obsidiandynamics/kafdrop)

### Creating a CDC stream in ksqldb-cli
TODO: Create stream as select with kafka.dotnet.ksqldb
```KSQL
CREATE STREAM IF NOT EXISTS sqlserversensors (
	op string,
	before STRUCT<SensorId VARCHAR, Value INT>,
	after STRUCT<SensorId VARCHAR, Value INT>,
	source STRUCT<Version VARCHAR, schema VARCHAR, "table" VARCHAR, "connector" VARCHAR>
  ) WITH (
    kafka_topic = 'sqlserver2019.dbo.Sensors',
    value_format = 'JSON'
);

SET 'auto.offset.reset' = 'earliest';
SELECT * FROM sqlserversensors EMIT CHANGES;
```

Sql server DML:
```SQL
INSERT INTO [dbo].[Sensors] ([SensorId], [Value])
VALUES ('734cac20-4', 33);

DELETE FROM [dbo].[Sensors] WHERE [SensorId] = '734cac20-4';

UPDATE [Sensors] SET [Value] = 45 WHERE [SensorId] = '02f8427c-6';
```

Output:
```
+----+-----------------------------------------+---------------------------------------+-----------------------------------------------------------------+
|OP  |BEFORE                                   |AFTER                                  |SOURCE                                                           |
+----+-----------------------------------------+---------------------------------------+-----------------------------------------------------------------+
|c   |null                                     |{SENSORID=734cac20-4, VALUE=33}        |{VERSION=1.5.0.Final, SCHEMA=dbo, table=Sensors, connector=sqlser|
|    |                                         |                                       |ver}                                                             |
|d   |{SENSORID=734cac20-4, VALUE=33}          |null                                   |{VERSION=1.5.0.Final, SCHEMA=dbo, table=Sensors, connector=sqlser|
|    |                                         |                                       |ver}                                                             |
|u   |{SENSORID=02f8427c-6, VALUE=1855}        |{SENSORID=02f8427c-6, VALUE=45}        |{VERSION=1.5.0.Final, SCHEMA=dbo, table=Sensors, connector=sqlser|
|    |                                         |                                       |ver}                                                             |  
```

### Consuming table change events directly from a kafka topic
```
Install-Package Kafka.DotNet.SqlServer -Version 0.1.0
Install-Package Kafka.DotNet.InsideOut -Version 1.0.0
Install-Package System.Interactive.Async -Version 5.0.0
```

```C#
async Task Main()
{
  string bootstrapServers = "localhost:29092";

  var consumerConfig = new ConsumerConfig
  {
    BootstrapServers = bootstrapServers,
    GroupId = "Client-01",
    AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
  };

  var kafkaConsumer =
    new KafkaConsumer<string, DatabaseChangeObject<IoTSensor>>("sqlserver2019.dbo.Sensors", consumerConfig);

  await foreach (var consumeResult in kafkaConsumer.ConnectToTopic().ToAsyncEnumerable().Take(10))
  {
    Console.WriteLine(consumeResult.Message);
  }

  using (kafkaConsumer)
}
```

### CdcClient (v0.1.0)
CdcEnableDbAsync - Enables change data capture for the current database. 

CdcEnableTableAsync - Enables change data capture for the specified source table in the current database.

```C#
using Kafka.DotNet.SqlServer.Cdc;

private static async Task TryEnableCdcAsync()
{
  var cdcClient = new CdcClient(connectionString);

  try
  {
    await cdcClient.CdcEnableDbAsync();
    
    if(!await CdcProvider.IsCdcTableEnabledAsync(tableName))
      await CdcProvider.CdcEnableTableAsync(tableName);
  }
  catch (Exception e)
  {
    Console.WriteLine(e);
  }
}
```

### IsCdcDbEnabledAsync and IsCdcTableEnabledAsync (v0.2.0)
IsCdcDbEnabledAsync - Has SQL Server database enabled Change Data Capture (CDC). 
IsCdcTableEnabledAsync - Has table Change Data Capture (CDC) enabled on a SQL Server database.

```C#
bool isCdcEnabled = await ClassUnderTest.IsCdcDbEnabledAsync(databaseName);
bool isTableCdcEnabled = await CdcProvider.IsCdcTableEnabledAsync(tableName)
```

### CdcEnableTable (v0.1.0)
Sql parameters for [sys.sp_cdc_enable_table](https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sys-sp-cdc-enable-table-transact-sql?view=sql-server-ver15#arguments)
Default schema name is 'dbo'.

```C#
string tableName = "Sensors";
string captureInstance = $"dbo_{tableName}_v2";

var cdcEnableTable = new CdcEnableTable(tableName)
{
  CaptureInstance = captureInstance,
  IndexName = "IX_Sensors_Name"
};

await CdcProvider.CdcEnableTableAsync(cdcEnableTable);
```

Optional arguments were added in (v0.2.0):
- IndexName, CaptureInstance, CapturedColumnList, FilegroupName

### SqlServerConnectorMetadata, ConnectorMetadata (v0.1.0)
SQL Server connector configuration example:
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

### KsqlDbConnect (v.0.1.0)
Creating the connector with ksqldb
```C#
using Kafka.DotNet.SqlServer.Connect;

private static async Task CreateConnectorAsync()
{
  var ksqlDbConnect = new KsqlDbConnect(new Uri(KsqlDbUrl));

  SqlServerConnectorMetadata connectorMetadata = CreateConnectorMetadata();

  await ksqlDbConnect.CreateConnectorAsync(connectorName: "MSSQL_SENSORS_CONNECTOR", connectorMetadata);
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

  var httpResponseMessage = await restApiClient.CreateStreamAsync<RawDatabaseChangeObject>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
    .ConfigureAwait(false);
}
```

### KsqlDbConnect - Drop Connectors (v0.2.0)
DropConnectorAsync - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement fails if the connector doesn't exist.
    
DropConnectorIfExistsAsync - Drop a connector and delete it from the Connect cluster. The topics associated with this cluster are not deleted by this command. The statement doesn't fail if the connector doesn't exist.

```C#
string connectorName = "MSSQL_SENSORS_CONNECTOR";

await ksqlDbConnect.DropConnectorAsync(connectorName);

await ksqlDbConnect.DropConnectorIfExistsAsync(connectorName);
```

### KsqlDbConnect - Get Connectors (v0.2.0)
GetConnectorsAsync - List all connectors in the Connect cluster.

```C#
var ksqlDbUrl = Configuration["ksqlDb:Url"];

var ksqlDbConnect = new KsqlDbConnect(new Uri(ksqlDbUrl));
      
var response = await ksqlDbConnect.GetConnectorsAsync();
```

### Disable CDC from.NET (v0.1.0)
CdcDisableTableAsync - Disables change data capture for the specified source table and capture instance in the current database.
CdcDisableDbAsync - Disables change data capture for the current database.
```C#
var cdcClient = new Kafka.DotNet.SqlServer.Cdc.CdcClient(connectionString);

await cdcClient.CdcDisableTableAsync(tableName, schemaName).ConfigureAwait(false);

await cdcClient.CdcDisableDbAsync().ConfigureAwait(false);
```

### `RawDatabaseChangeObject<TEntity>` (v.0.1.0)
```C#
using System;
using System.Reactive;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.SqlServer.Cdc;

await using var context = new KSqlDBContext(@"http:\\localhost:8088");

context.CreateQuery<RawDatabaseChangeObject<IoTSensor>>("sqlserversensors")
  .Subscribe(new AnonymousObserver<RawDatabaseChangeObject<IoTSensor>>(dco =>
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

### Debezium connector for Sql Server
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