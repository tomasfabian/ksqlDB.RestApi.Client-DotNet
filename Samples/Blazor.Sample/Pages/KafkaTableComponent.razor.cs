using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Kafka.DotNet.InsideOut.Consumer;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Blazor.Sample.Pages
{
  public partial class KafkaTableComponent : IDisposable
  {
    [Inject]
    private IConfiguration Configuration { get; init; }

    [Inject]
    private IKafkaConsumer<string, IoTSensorStats> ItemsTableConsumer { get; init; }

    private readonly CancellationTokenSource cancellationTokenSource = new();
    
    private string KsqlDbUrl => Configuration[ConfigKeys.KSqlDb_Url];

    protected override async Task OnInitializedAsync()
    {
      await CreateTableAsync();

      var synchronizationContext = SynchronizationContext.Current;

      var options = new KSqlDBContextOptions(KsqlDbUrl)
      {
        ShouldPluralizeFromItemName = false
      };
      
      await using var context = new KSqlDBContext(options);

      context.CreateQuery<IoTSensorStats>(TopicNames.SensorsTable)
        .ToObservable()
        .ObserveOn(synchronizationContext)
        .Subscribe(c =>
        {
          items[c.SensorId] = c;

          StateHasChanged();
        }, error =>
        {
        });

      await base.OnInitializedAsync();
    }

    private async Task CreateTableAsync()
    {
      string ksqlDbUrl = Configuration[ConfigKeys.KSqlDb_Url];

      await using var context = new KSqlDBContext(ksqlDbUrl);

      var statement = context.CreateOrReplaceTableStatement(tableName: TopicNames.SensorsTable)
        .As<IoTSensor>(TopicNames.IotSensors)
        .Where(c => c.SensorId != "Sensor-5")
        .GroupBy(c => c.SensorId)
        .Select(c => new {SensorId = c.Key, Count = c.Count(), Sum = c.Sum(a => a.Value), LatestByOffset = c.LatestByOffset(a => a.Value, 2) });

      var httpResponseMessage = await statement.ExecuteStatementAsync(cancellationTokenSource.Token);

      if (httpResponseMessage.IsSuccessStatusCode)
      {
        var statementResponses = httpResponseMessage.ToStatementResponses();
      }
      else
      {
        var statementResponse = httpResponseMessage.ToStatementResponse();
      }
    }
    
    private IDisposable subscription;
    
    private void SubscribeToSensors(SynchronizationContext synchronizationContext)
    {
      subscription = ItemsTableConsumer.ConnectToTopic(timeout: null, cancellationTokenSource.Token)
        .ToObservable()
        .SubscribeOn(NewThreadScheduler.Default)
        .ObserveOn(synchronizationContext)
        .Subscribe(c =>
        {
          c.Value.SensorId = c.Key;

          items[c.Key] = c.Value;

          StateHasChanged();

          Console.WriteLine($"{c.Key} - {c.Value.Count}");
        }, error => { });
    }

    public void Dispose()
    {         
      cancellationTokenSource.Cancel();
      cancellationTokenSource.Dispose();
      
      subscription?.Dispose();
    }
  }
}