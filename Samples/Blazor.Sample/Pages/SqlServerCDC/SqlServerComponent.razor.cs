using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data;
using Blazor.Sample.Data.Sensors;
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
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Blazor.Sample.Pages.SqlServerCDC
{
  public partial class SqlServerComponent : IDisposable
  {
    [Inject] private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; init; }

    [Inject] private IConfiguration Configuration { get; init; }

    [Inject] private ICdcClient CdcProvider { get; init; }

    private int TotalCount => sensors.Count;

    protected override async Task OnInitializedAsync()
    {
      SetNewModel();

      var dbContext = DbContextFactory.CreateDbContext();

      sensors = await EntityFrameworkQueryableExtensions.ToListAsync(dbContext.Sensors);

      const string tableName = "Sensors";

      await CreateSensorsCdcStreamAsync();

      await EnableCdcAsync(tableName);

      await CreateConnectorAsync(tableName);

      //await CreateSensorsChangeDataCaptureStreamAsync();

      var synchronizationContext = SynchronizationContext.Current;

      await SubscribeToQuery(synchronizationContext);

      await base.OnInitializedAsync();
    }

    private async Task EnableCdcAsync(string tableName)
    {
      try
      {
        await CdcProvider.EnableAsync(tableName);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    private async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
    {
      await using var context = new KSqlDBContext(KsqlDbUrl);

      string fromName = "sqlserversensors";
      string kafkaTopic = "sqlserver2019.dbo.Sensors";

      var ksqlDbUrl = Configuration[ConfigKeys.KSqlDb_Url];

      var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

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

    /// <summary>
    /// Create stream from string statement example
    /// </summary>
    private async Task CreateSensorsChangeDataCaptureStreamAsync()
    {
      var createSensorsCDCStream = @"
CREATE STREAM IF NOT EXISTS sqlserversensors (
    op string, before string, after string, source string
  ) WITH (
    kafka_topic = 'sqlserver2019.dbo.Sensors',
    value_format = 'JSON'
);";

      var ksqlDbStatement = new KSqlDbStatement(createSensorsCDCStream);

      var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement);
    }

    /// <summary>
    /// Create connector from metadata example
    /// </summary>
    private async Task CreateConnectorAsync(string tableName, string schemaName = "dbo")
    {
      string bootstrapServers = Configuration[ConfigKeys.Kafka_BootstrapServers];
      var connectionString = Configuration.GetConnectionString("DefaultConnection");

      var connectorMetadata = new ConnectorMetadata(connectionString)
        .SetJsonKeyConverter()
        .SetJsonValueConverter()
        .SetTableIncludeListPropertyName($"{schemaName}.{tableName}")
        .SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
        .SetProperty("database.history.kafka.topic", $"dbhistory.{tableName}")
        .SetProperty("database.server.name", "sqlserver2019")
        .SetProperty("key.converter.schemas.enable", "false")
        .SetProperty("value.converter.schemas.enable", "false")
        .SetProperty("include.schema.changes", "false")
        .SetTableIncludeListPropertyName($"{schemaName}.{tableName}");

      var createConnector = connectorMetadata.ToStatement(connectorName: "MSSQL_SENSORS_CONNECTOR");

      KSqlDbStatement ksqlDbStatement = new(createConnector);

      var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement).ConfigureAwait(false);
    }

    /// <summary>
    /// Create connector from string statement example
    /// </summary>
    private async Task CreateConnectorFromStringAsync(string tableName, string schemaName = "dbo")
    {
      string bootstrapServers = Configuration[ConfigKeys.Kafka_BootstrapServers];

      var createConnector = @$"CREATE SOURCE CONNECTOR MSSQL_SENSORS_CONNECTOR WITH (
  'connector.class' = 'io.debezium.connector.sqlserver.SqlServerConnector',
  'database.hostname'= 'sqlserver2019', 
  'database.port'= '1433',
  'database.user'= 'sa', 
  'database.password'= '<YourNewStrong@Passw0rd>', 
  'database.dbname'= 'Sensors', 
  'database.server.name'= 'sqlserver2019', 
  'table.include.list'= '{schemaName}.{tableName}', 
  'database.history.kafka.bootstrap.servers'= '{bootstrapServers}', 
  'database.history.kafka.topic'= 'dbhistory.{tableName}',
  'key.converter'= 'org.apache.kafka.connect.json.JsonConverter',
  'key.converter.schemas.enable'= 'false',
  'value.converter'= 'org.apache.kafka.connect.json.JsonConverter',
  'value.converter.schemas.enable'= 'false',
  'include.schema.changes'= 'false'
);";

      KSqlDbStatement ksqlDbStatement = new(createConnector);

      var httpResponseMessage = await ExecuteStatementAsync(ksqlDbStatement);
    }

    public Task<HttpResponseMessage> ExecuteStatementAsync(KSqlDbStatement ksqlDbStatement, CancellationToken cancellationToken = default)
    {
      var ksqlDbUrl = Configuration[ConfigKeys.KSqlDb_Url];

      var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

      var restApiClient = new KSqlDbRestApiClient(httpClientFactory);

      return restApiClient.ExecuteStatementAsync(ksqlDbStatement, cancellationToken);
    }

    private string KsqlDbUrl => Configuration[ConfigKeys.KSqlDb_Url];

    private IDisposable cdcSubscription;

    private async Task SubscribeToQuery(SynchronizationContext? synchronizationContext)
    {
      var options = new KSqlDBContextOptions(KsqlDbUrl)
      {
        ShouldPluralizeFromItemName = false
      };

      await using var context = new KSqlDBContext(options);

      cdcSubscription = context.CreateQuery<DatabaseChangeObject>("sqlserversensors")
        .WithOffsetResetPolicy(AutoOffsetReset.Latest)
        .ToObservable()
        .ObserveOn(synchronizationContext)
        .Subscribe(cdc =>
        {
          items.Enqueue(cdc);

          UpdateTable(cdc);

          StateHasChanged();
        }, error => { });
    }

    private void UpdateTable(DatabaseChangeObject databaseChangeObject)
    {
      switch (databaseChangeObject.Op.ToChangeDataCaptureType())
      {
        case ChangeDataCaptureType.Created:
          var sensor = JsonSerializer.Deserialize<IoTSensor>(databaseChangeObject.After);
          
          var existing = sensors.FirstOrDefault(c => c.SensorId == sensor.SensorId);

          if (existing == null)
            sensors.Add(sensor);
          break;

        case ChangeDataCaptureType.Updated:
          TryUpdateSensor(databaseChangeObject);

          break;

        case ChangeDataCaptureType.Deleted:
          var sensorBefore = JsonSerializer.Deserialize<IoTSensor>(databaseChangeObject.Before);
          var itemToRemove = sensors.FirstOrDefault(c => c.SensorId == sensorBefore.SensorId);
          if (itemToRemove != null)
            sensors.Remove(itemToRemove);
          break;
      }
    }

    private void TryUpdateSensor(DatabaseChangeObject databaseChangeObject)
    {
      var sensorAfter = JsonSerializer.Deserialize<IoTSensor>(databaseChangeObject.After);

      var found = sensors.FirstOrDefault(c => c.SensorId == sensorAfter.SensorId);
      var index = sensors.IndexOf(found);

      if (index != -1)
        sensors[index] = sensorAfter;
      else
        sensors.Add(sensorAfter);
    }

    private string TranslateOperation(string operation)
    {
      return operation switch
      {
        "c" => "Created",
        "u" => "Updated",
        "d" => "Deleted",
        "r" => "Read",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
      };
    }

    private IoTSensor Model { get; set; } 

    private async Task SaveAsync()
    {
      var dbContext = DbContextFactory.CreateDbContext();

      dbContext.Sensors.Add(Model);

      await dbContext.SaveChangesAsync();
      
      SetNewModel();
    }
    private async Task DeleteAsync(IoTSensor sensor)
    {
      var dbContext = DbContextFactory.CreateDbContext();

      dbContext.Sensors.Remove(sensor);

      await dbContext.SaveChangesAsync();
    }

    private void SetNewModel()
    {
      Model = new IoTSensor
      {
        SensorId = Guid.NewGuid().ToString().Substring(0, 10)
      };
    }

    public void Dispose()
    {
      cdcSubscription?.Dispose();
    }
  }
}