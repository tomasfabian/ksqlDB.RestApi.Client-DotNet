using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Providers;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.Query.Options;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Blazor.Sample.Pages.SqlServerCDC
{
  public partial class SqlServerComponent
  {
    [Inject] private ApplicationDbContext DbContext { get; init; }

    [Inject] private IConfiguration Configuration { get; init; }

    [Inject] private ISqlServerChangeDataCaptureProvider CdcProvider { get; init; }

    protected override async Task OnInitializedAsync()
    {
      SetNewModel();

      sensors = await DbContext.Sensors.ToListAsync();

      const string tableName = "Sensors";

      await CdcProvider.EnableAsync(tableName);

      await CreateConnectorAsync(tableName);

      await CreateSensorsChangeDataCaptureStreamAsync();

      var synchronizationContext = SynchronizationContext.Current;

      await SubscribeToQuery(synchronizationContext);

      await base.OnInitializedAsync();
    }

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
      if (httpResponseMessage.IsSuccessStatusCode)
      {
        StatementResponse[] statementResponses = httpResponseMessage.ToStatementResponses();
      }
      else
      {
        StatementResponse statementResponse = httpResponseMessage.ToStatementResponse();
      }
    }

    private async Task CreateConnectorAsync(string tableName, string schemaName = "dbo")
    {
      string bootstrapServers= Configuration[ConfigKeys.Kafka_BootstrapServers];

      //TODO: extract database from config
      var createConnector = @$"CREATE SOURCE CONNECTOR MSSQL_SENSORS WITH (
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

    private async Task SubscribeToQuery(SynchronizationContext? synchronizationContext)
    {
      var options = new KSqlDBContextOptions(KsqlDbUrl)
      {
        ShouldPluralizeFromItemName = false
      };

      await using var context = new KSqlDBContext(options);

      context.CreateQuery<DatabaseChangeObject>("sqlserversensors")
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
      switch (ToChangeDataCaptureType(databaseChangeObject.Op))
      {
        case ChangeDataCaptureType.Created:
          var sensor = JsonSerializer.Deserialize<IoTSensor>(databaseChangeObject.After);
          
          var existing = sensors.FirstOrDefault(c => c.SensorId == sensor.SensorId);

          if (existing == null)
            sensors.Add(sensor);
          break;

        case ChangeDataCaptureType.Updated:
          var sensorAfter = JsonSerializer.Deserialize<IoTSensor>(databaseChangeObject.After);
          break;

        case ChangeDataCaptureType.Deleted:
          var sensorBefore = JsonSerializer.Deserialize<IoTSensor>(databaseChangeObject.Before);
          var itemToRemove = sensors.FirstOrDefault(c => c.SensorId == sensorBefore.SensorId);
          if (itemToRemove != null)
            sensors.Remove(itemToRemove);
          break;
      }
    }

    private ChangeDataCaptureType ToChangeDataCaptureType(string operation)
    {
      return operation switch
      {
        "r" => ChangeDataCaptureType.Read,
        "c" => ChangeDataCaptureType.Created,
        "u" => ChangeDataCaptureType.Updated,
        "d" => ChangeDataCaptureType.Deleted,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
      };
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
      DbContext.Sensors.Add(Model);

      await DbContext.SaveChangesAsync();
      
      SetNewModel();
    }
    private async Task DeleteAsync(IoTSensor sensor)
    {
      DbContext.Sensors.Remove(sensor);

      await DbContext.SaveChangesAsync();
    }

    private void SetNewModel()
    {
      Model = new IoTSensor
      {
        SensorId = Guid.NewGuid().ToString().Substring(0, 10)
      };
    }
  }
}