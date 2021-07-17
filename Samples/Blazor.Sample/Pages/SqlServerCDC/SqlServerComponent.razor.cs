using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data;
using Blazor.Sample.Data.Sensors;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.SqlServer.Cdc;
using Kafka.DotNet.SqlServer.Cdc.Connectors;
using Kafka.DotNet.SqlServer.Connect;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoOffsetReset = Kafka.DotNet.ksqlDB.KSql.Query.Options.AutoOffsetReset;

namespace Blazor.Sample.Pages.SqlServerCDC
{
  public partial class SqlServerComponent : IDisposable
  {
    [Inject] private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; init; }

    [Inject] private IConfiguration Configuration { get; init; }

    [Inject] private ISqlServerCdcClient CdcProvider { get; init; }

    [Inject] private IKsqlDbConnect KsqlDbConnect { get; init; }

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

      await SubscribeToQuery(synchronizationContext);

      await base.OnInitializedAsync();
    }

    private async Task LoadDataFromDbAsync()
    {
      IsLoading = true;

      try
      {
        var dbContext = DbContextFactory.CreateDbContext();

        sensors = await EntityFrameworkQueryableExtensions.ToListAsync(dbContext.Sensors);
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

        if(!await CdcProvider.IsCdcTableEnabledAsync(tableName))
          await CdcProvider.CdcEnableTableAsync(tableName);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    private async Task CreateSensorsCdcStreamAsync(CancellationToken cancellationToken = default)
    {
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
      
      var httpResponseMessage = await restApiClient.CreateStreamAsync<RawDatabaseChangeObject>(metadata, ifNotExists: true, cancellationToken: cancellationToken)
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

      cdcSubscription = context.CreateQuery<RawDatabaseChangeObject<IoTSensor>>("sqlserversensors")
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

    private void UpdateTable(RawDatabaseChangeObject<IoTSensor> rawDatabaseChangeObject)
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

    private void TryUpdateSensor(RawDatabaseChangeObject<IoTSensor> rawDatabaseChangeObject)
    {
      var sensorAfter = rawDatabaseChangeObject.EntityAfter;

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
    
    private async Task UpdateAsync(IoTSensor sensor)
    {
      if(sensor == null)
        return;

      var dbContext = DbContextFactory.CreateDbContext();

      var updatedSensor = sensor with {Value = new Random().Next(1, 100)};

      dbContext.Sensors.Update(updatedSensor);

      await dbContext.SaveChangesAsync();
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

    async Task ConsumeFromTopicExampleAsync()
    {
      var consumerConfig = new ConsumerConfig
      {
        BootstrapServers = Configuration[ConfigKeys.Kafka_BootstrapServers],
        GroupId = "BlazorClient-01",
        AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
      };

      var kafkaConsumer = new KafkaConsumer<string, DatabaseChangeObject<IoTSensor>>("sqlserver2019.dbo.Sensors", consumerConfig);
	
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
}