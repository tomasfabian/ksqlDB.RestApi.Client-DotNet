using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Extensions.Http;
using Blazor.Sample.Kafka;
using InsideOut.Consumer;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
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
      //!!! disclaimer - these steps shouldn't be part of a component initialization. It is intended only for demonstration purposes, to see the relevant parts together.
      string ksqlDbUrl = Configuration[ConfigKeys.KSqlDb_Url];

      await using var context = new KSqlDBContext(ksqlDbUrl);

      var statement = context.CreateOrReplaceTableStatement(tableName: TopicNames.SensorsTable)
        .As<IoTSensor>(TopicNames.IotSensors)
        .Where(c => c.SensorId != "Sensor-5")
        .GroupBy(c => c.SensorId)
        .Select(c => new {SensorId = c.Key, Count = c.Count(), Sum = c.Sum(a => a.Value), LatestByOffset = c.LatestByOffset(a => a.Value, 2) });

      var httpResponseMessage = await statement.ExecuteStatementAsync(cancellationTokenSource.Token);
      
      var statementResponses = httpResponseMessage.ConvertToStatementResponses();
      //!!! disclaimer
    }
    
    private IDisposable subscription;
    
    private void SubscribeToSensors(SynchronizationContext synchronizationContext)
    {
      subscription = ItemsTableConsumer.ConnectToTopic()
        .ToObservable()
        .Select(c => c.Message)
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

      ItemsTableConsumer.Dispose();
    }
  }
}