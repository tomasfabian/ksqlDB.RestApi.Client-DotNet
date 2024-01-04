using System.Reactive.Linq;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Components.Pages.SqlServerCDC.Models;
using Confluent.Kafka;
using InsideOut.Consumer;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Cdc.Connectors;
using SqlServer.Connector.Connect;
using AutoOffsetReset = ksqlDB.RestApi.Client.KSql.Query.Options.AutoOffsetReset;

namespace Blazor.Sample.Components.Pages.SqlServerCDC;

public partial class SqlServerComponent : IDisposable
{
  [Inject] private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; init; }

  [Inject] private IConfiguration Configuration { get; init; }

  [Inject] private ISqlServerCdcClient CdcProvider { get; init; }

  [Inject] private IKsqlDbConnect KsqlDbConnect { get; init; }
  
  [Inject] private IKSqlDbRestApiClient KSqlDbRestApiClient { get; init; }

  private int TotalCount => sensors.Count;

  private bool IsLoading { get; set; }

  protected override async Task OnInitializedAsync()
  {
    SetNewModel();

    await LoadDataFromDbAsync();

    const string tableName = "Sensors";

    //!!! disclaimer - these steps shouldn't be part of a component initialization. It is intended only for demonstration purposes, to see the relevant parts together.
    await CreateSensorsCdcStreamAsync();

    await EnableCdcAsync(tableName);

    await CreateConnectorAsync(tableName);
    //!!! disclaimer

    var connectorsResponse = await KsqlDbConnect.GetConnectorsAsync();
    var connectors = await connectorsResponse.ToConnectorsResponseAsync();

    var synchronizationContext = SynchronizationContext.Current;

    if (useKsqlDbTypes)
      await SubscribeToQuery(synchronizationContext);
    else
      await SubscribeToRawQuery(synchronizationContext);

    await base.OnInitializedAsync();
  }

  private async Task LoadDataFromDbAsync()
  {
    IsLoading = true;

    try
    {
      var dbContext = await DbContextFactory.CreateDbContextAsync();

      sensors = await dbContext.Sensors.ToListAsync();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }
    finally
    {
      IsLoading = false;
    }
  }

  private async Task EnableCdcAsync(string tableName)
  {
    try
    {
      await CdcProvider.CdcEnableDbAsync();

      if (!await CdcProvider.IsCdcTableEnabledAsync(tableName))
        await CdcProvider.CdcEnableTableAsync(tableName);
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }
  }

  private bool useKsqlDbTypes = true;

  private async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
  {
    string fromName = "sqlserversensors";
    string kafkaTopic = "sqlserver2019.dbo.Sensors";

    EntityCreationMetadata metadata = new()
    {
      EntityName = fromName,
      KafkaTopic = kafkaTopic,
      ValueFormat = SerializationFormats.Json,
      Partitions = 1,
      Replicas = 1
    };

    if (useKsqlDbTypes)
    {
      var createTypeResponse = await KSqlDbRestApiClient.CreateTypeAsync<IoTSensor>(cancellationToken);
      createTypeResponse = await KSqlDbRestApiClient.CreateTypeAsync<IoTSensorChange>(cancellationToken);
        
      var httpResponseMessage = await KSqlDbRestApiClient.CreateStreamAsync<IoTSensorChange>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    }
    else
    {
      var httpResponseMessage = await KSqlDbRestApiClient.CreateStreamAsync<RawDatabaseChangeObject>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    }
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

    var httpResponseMessage = await KSqlDbRestApiClient.ExecuteStatementAsync(ksqlDbStatement);
  }

  /// <summary>
  /// Create connector from metadata example
  /// </summary>
  private async Task CreateConnectorAsync(string tableName, string schemaName = "dbo")
  {
    string bootstrapServers = Configuration[ConfigKeys.Kafka_BootstrapServers];
    var connectionString = Configuration.GetConnectionString("DefaultConnection");

    var connectorMetadata = new SqlServerConnectorMetadata(connectionString)
      .SetTableIncludeListPropertyName($"{schemaName}.{tableName}")
      .SetJsonKeyConverter()
      .SetJsonValueConverter()
      .SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
      .SetProperty("database.history.kafka.topic", $"dbhistory.{tableName}")
      .SetProperty("database.server.name", "sqlserver2019")
      .SetProperty("key.converter.schemas.enable", "false")
      .SetProperty("value.converter.schemas.enable", "false")
      .SetProperty("include.schema.changes", "false") as SqlServerConnectorMetadata;

    var httpResponseMessage = await KsqlDbConnect.CreateConnectorAsync(connectorName: "MSSQL_SENSORS_CONNECTOR", connectorMetadata);
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

    var httpResponseMessage = await KSqlDbRestApiClient.ExecuteStatementAsync(ksqlDbStatement);
  }

  private string KsqlDbUrl => Configuration[ConfigKeys.KSqlDb_Url];

  private IDisposable cdcSubscription;

  private async Task SubscribeToQuery(SynchronizationContext synchronizationContext)
  {
    var options = new KSqlDBContextOptions(KsqlDbUrl)
    {
      ShouldPluralizeFromItemName = false
    };

    await using var context = new KSqlDBContext(options);

    cdcSubscription = context.CreateQuery<IoTSensorChange>("sqlserversensors")
      .WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .Where(c => c.Op != "r" && (c.After == null || c.After.SensorId != "d542a2b3-c"))
      .ToObservable()
      .ObserveOn(synchronizationContext)
      .Subscribe(cdc =>
      {
        items.Enqueue(cdc);
          
        UpdateTable(cdc);

        StateHasChanged();
      }, error => { Console.WriteLine(error.Message); });
  }

  private async Task SubscribeToRawQuery(SynchronizationContext synchronizationContext)
  {
    var options = new KSqlDBContextOptions(KsqlDbUrl)
    {
      ShouldPluralizeFromItemName = false
    };

    await using var context = new KSqlDBContext(options);

    cdcSubscription = context.CreateQuery<IoTSensorRawChange>("sqlserversensors")
      .WithOffsetResetPolicy(AutoOffsetReset.Latest)
      .ToObservable()
      .ObserveOn(synchronizationContext)
      .Subscribe(cdc =>
      {
        items.Enqueue(cdc);

        UpdateTable(cdc);

        StateHasChanged();
      }, error => { Console.WriteLine(error.Message); });
  }

  private void UpdateTable(IDbRecord<IoTSensor> rawDatabaseChangeObject)
  {
    switch (rawDatabaseChangeObject.OperationType)
    {
      case ChangeDataCaptureType.Created:
        var sensor = rawDatabaseChangeObject.EntityAfter;

        var existing = sensors.FirstOrDefault(c => c.SensorId == sensor.SensorId);

        if (existing == null)
          sensors.Add(sensor);
        break;

      case ChangeDataCaptureType.Updated:
        TryUpdateSensor(rawDatabaseChangeObject);

        break;

      case ChangeDataCaptureType.Deleted:
        var sensorBefore = rawDatabaseChangeObject.EntityBefore;
        var itemToRemove = sensors.FirstOrDefault(c => c.SensorId == sensorBefore.SensorId);
        if (itemToRemove != null)
          sensors.Remove(itemToRemove);
        break;
    }
  }

  private void TryUpdateSensor(IDbRecord<IoTSensor> dbRecord)
  {
    var sensorAfter = dbRecord.EntityAfter;

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
    var dbContext = await DbContextFactory.CreateDbContextAsync();

    dbContext.Sensors.Add(Model);

    await dbContext.SaveChangesAsync();

    SetNewModel();
  }

  private async Task UpdateAsync(IoTSensor sensor)
  {
    if (sensor == null)
      return;

    var dbContext = await DbContextFactory.CreateDbContextAsync();

    var updatedSensor = sensor with { Value = new Random().Next(1, 100) };

    dbContext.Sensors.Update(updatedSensor);

    await dbContext.SaveChangesAsync();
  }

  private async Task DeleteAsync(IoTSensor sensor)
  {
    var dbContext = await DbContextFactory.CreateDbContextAsync();

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

  async Task ConsumeFromTopicExampleAsync()
  {
    var consumerConfig = new ConsumerConfig
    {
      BootstrapServers = Configuration[ConfigKeys.Kafka_BootstrapServers],
      GroupId = "BlazorClient-01",
      AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
    };

    var kafkaConsumer = new KafkaConsumer<string, Models.DatabaseChangeObject<IoTSensor>>("sqlserver2019.dbo.Sensors", consumerConfig);

    await foreach (var consumeResult in kafkaConsumer.ConnectToTopic().ToAsyncEnumerable().Take(10))
    {
      Console.WriteLine(consumeResult.Message);
    }

    using (kafkaConsumer)
    { }
  }

  public void Dispose()
  {
    cdcSubscription?.Dispose();
  }
}
