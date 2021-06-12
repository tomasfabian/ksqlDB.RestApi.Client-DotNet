using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Configuration;
using Blazor.Sample.Data.Sensors;
using Blazor.Sample.Kafka;
using Confluent.Kafka;
using Kafka.DotNet.InsideOut.Consumer;
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

    protected override async Task OnInitializedAsync()
    {
      var adminClient = GetAdminClient();

      var brokerMetadata = adminClient.GetMetadata(TimeSpan.FromSeconds(3));

      await CreateItemsStreamAsync();

      topicSubscription = ItemsConsumer.ConnectToTopicAsync()
        .ObserveOn(SynchronizationContext.Current)
        .Subscribe(c =>
        {
          c.Value.Id = c.Key;

          items.Enqueue(c.Value);
          StateHasChanged();
        }, error =>
        {

        });

      await base.OnInitializedAsync();
    }
    
    private async Task CreateItemsStreamAsync()
    {
      await using var context = new KSqlDBContext(ksqlDbUrl);

      var statement = context.CreateOrReplaceStreamStatement(streamName: TopicNames.SensorsStream)
        .As<IoTSensor>()
        .Select(c => new { c.SensorId, c.Value })
        .PartitionBy(c => c.SensorId);

      var httpResponseMessage = await statement.ExecuteStatementAsync();

      if (httpResponseMessage.IsSuccessStatusCode)
      {
        StatementResponse[] statementResponses = httpResponseMessage.ToStatementResponses();
      }
      else
      {
        StatementResponse statementResponse = httpResponseMessage.ToStatementResponse();
      }
    }

    private string ksqlDbUrl => Configuration[ConfigKeys.KSqlDb_Url];
    
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
      topicSubscription?.Dispose();
    }
  }
}