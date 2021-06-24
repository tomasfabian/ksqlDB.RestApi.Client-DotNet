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
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Blazor.Sample.Pages
{
  partial class KafkaStreamComponent : IDisposable
  {
    [Inject]
    private IConfiguration Configuration { get; init; }

    [Inject]
    private IKafkaConsumer<string, SensorsStream> ItemsConsumer { get; init; }

    private IDisposable topicSubscription;
    
    private CancellationTokenSource cancellationTokenSource = new();

    protected override async Task OnInitializedAsync()
    {
      var adminClient = GetAdminClient();

      var brokerMetadata = adminClient.GetMetadata(TimeSpan.FromSeconds(3));

      await CreateItemsStreamAsync();

      var synchronizationContext = SynchronizationContext.Current;

      // await SubscribeToQuery(synchronizationContext);

      SubscribeToSensors(synchronizationContext);

      await base.OnInitializedAsync();
    }
    private async Task CreateItemsStreamAsync()
    {
      await using var context = new KSqlDBContext(KsqlDbUrl);

      var statement = context.CreateOrReplaceStreamStatement(streamName: TopicNames.SensorsStream)
        .As<IoTSensor>()
        .Select(c => new { c.SensorId, c.Value })
        .PartitionBy(c => c.SensorId);

      var httpResponseMessage = await statement.ExecuteStatementAsync(cancellationTokenSource.Token);

      if (httpResponseMessage.IsSuccessStatusCode)
      {
        StatementResponse[] statementResponses = httpResponseMessage.ToStatementResponses();
      }
      else
      {
        StatementResponse statementResponse = httpResponseMessage.ToStatementResponse();
      }
    }

    private async Task SubscribeToQuery(SynchronizationContext? synchronizationContext)
    {
      var options = new KSqlDBContextOptions(KsqlDbUrl)
      {
        ShouldPluralizeFromItemName = false
      };

      await using var context = new KSqlDBContext(options);

      context.CreateQuery<SensorsStream>("SensorsStream")
        .ToObservable()
        .ObserveOn(synchronizationContext)
        .Subscribe(c =>
        {
          c.Id = c.Id;

          items.Enqueue(c);
          StateHasChanged();
        }, error => { });
    }

    private void SubscribeToSensors(SynchronizationContext synchronizationContext)
    {
      topicSubscription = ItemsConsumer.ConnectToTopic()
        .ToObservable()
        .SubscribeOn(NewThreadScheduler.Default)
        .ObserveOn(synchronizationContext)
        .Subscribe(c =>
        {
          c.Value.Id = c.Key;

          items.Enqueue(c.Value);
          StateHasChanged();
        }, error => { Console.WriteLine(error.Message); });
    }

    private string KsqlDbUrl => Configuration[ConfigKeys.KSqlDb_Url];
    
    public IAdminClient GetAdminClient()
    {
      var config = new AdminClientConfig
      {
        BootstrapServers = Configuration["Kafka:BootstrapServers"]
      };

      var adminClientBuilder = new AdminClientBuilder(config);

      var adminClient = adminClientBuilder.Build();

      return adminClient;
    }

    public void Dispose()
    {
      cancellationTokenSource.Cancel();
      cancellationTokenSource.Dispose();

      topicSubscription?.Dispose();

      ItemsConsumer.Dispose();
    }
  }
}