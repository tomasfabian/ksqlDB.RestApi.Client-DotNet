using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
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

    protected override async Task OnInitializedAsync()
    {
      var sensors = await DbContext.Sensors.ToListAsync();

      await CreateConnectorAsync();

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

    private async Task CreateConnectorAsync()
    {
      var createConnector = @$"CREATE SOURCE CONNECTOR MSSQL_SENSORS WITH (

  'connector.class' = 'io.debezium.connector.sqlserver.SqlServerConnector',
  'database.hostname'= 'sqlserver2019', 
  'database.port'= '1433', 
  'database.user'= 'sa', 
  'database.password'= '<YourNewStrong@Passw0rd>', 
  'database.dbname'= 'Sensors', 
  'database.server.name'= 'sqlserver2019', 
  'table.include.list'= 'dbo.Sensors', 
  'database.history.kafka.bootstrap.servers'= 'broker01:9092', 
  'database.history.kafka.topic'= 'dbhistory.sensors',
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
        .ToObservable()
        .ObserveOn(synchronizationContext)
        .Subscribe(c =>
        {
          items.Enqueue(c);

          StateHasChanged();
        }, error => { });
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
  }
}