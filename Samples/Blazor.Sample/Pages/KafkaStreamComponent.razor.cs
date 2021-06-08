using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Data;
using Blazor.Sample.Kafka;
using Confluent.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;
using Kafka.DotNet.ksqlDB.InsideOut.Producer;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.AspNetCore.Components;

namespace Blazor.Sample.Pages
{
  partial class KafkaStreamComponent : IDisposable
  {
    [Inject]
    private IKafkaConsumer<int, ItemStream> ItemsConsumer { get; init; }


    [Inject]
    private IKafkaProducer<int, Item> ItemsProducer { get; init; }

    private IDisposable topicSubscription;
    private IDisposable timerSubscription;

    protected override async Task OnInitializedAsync()
    {
      var adminClient = GetAdminClient();

      var brokerMetadata = adminClient.GetMetadata(TimeSpan.FromSeconds(3));

      await TryCreateStreamAsync();

      await CreateItemsStreamAsync();

      topicSubscription = ItemsConsumer.ConnectToTopicAsync()
        .ObserveOn(SynchronizationContext.Current)
        .Subscribe(c =>
        {
          c.Value.Id = c.Key;

          items.Add(c.Value);
          StateHasChanged();
        }, error =>
        {

        });

      int key = 0;

      timerSubscription =
        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
          .Subscribe(async c =>
          {
            key++;
            var deliveryResult = await ItemsProducer.ProduceMessageAsync(key, new Item { Id = key, Description = $"Desc {key}" });

            if (key >= 4)
              key = 0;
          });

      await base.OnInitializedAsync();
    }

    string bootstrapServers = "broker01:9092";

    private static async Task CreateItemsStreamAsync()
    {
      await using var context = new KSqlDBContext(ksqlDbUrl);

      var statement = context.CreateOrReplaceStreamStatement(streamName: TopicNames.ItemsStream)
        .As<Item>()
        .Select(c => new { c.Id, c.Description })
        .PartitionBy(c => c.Id);

      var httpResponseMessage = await statement.ExecuteStatementAsync();

      if (!httpResponseMessage.IsSuccessStatusCode)
      {
        var statementResponse = httpResponseMessage.ToStatementResponse();
      }
      else
      {
        var statementResponses = httpResponseMessage.ToStatementResponses();
      }
    }

    public static string ksqlDbUrl = @"http://ksqldb-server:8088";

    private static async Task TryCreateStreamAsync()
    {
      EntityCreationMetadata metadata = new()
      {
        KafkaTopic = nameof(TopicNames.Items),
        Partitions = 1,
        Replicas = 1
      };

      var http = new HttpClientFactory(new Uri(ksqlDbUrl));
      var restApiClient = new KSqlDbRestApiClient(http);

      var httpResponseMessage = await restApiClient.CreateStreamAsync<Item>(metadata, ifNotExists: true);
    }

    public IAdminClient GetAdminClient()
    {

      var config = new AdminClientConfig
      {
        BootstrapServers = bootstrapServers
      };

      var adminClientBuilder = new AdminClientBuilder(config);

      var adminClient = adminClientBuilder.Build();

      return adminClient;
    }

    public void Dispose()
    {
      topicSubscription?.Dispose();
      timerSubscription?.Dispose();
    }
  }
}