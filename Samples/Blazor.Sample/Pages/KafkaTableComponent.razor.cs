using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Sample.Data;
using Blazor.Sample.Kafka;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;
using Kafka.DotNet.ksqlDB.KSql.Linq.Statements;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Blazor.Sample.Pages
{
  public partial class KafkaTableComponent
  {
    [Inject]
    private IConfiguration Configuration { get; init; }

    [Inject]
    private IKafkaConsumer<int, ItemTable> ItemsTableConsumer { get; init; }

    protected override async Task OnInitializedAsync()
    {
      await CreateTableAsync();

      ItemsTableConsumer.ConnectToTopicAsync()
        .ObserveOn(SynchronizationContext.Current)
        .Subscribe(c =>
        {
          Console.WriteLine($"{c.Key} - {c.Value.Count}");
        }, error =>
        {

        });

      await base.OnInitializedAsync();
    }

    private async Task CreateTableAsync()
    {
      string ksqlDbUrl = Configuration["ksqlDb:Url"];

      await using var context = new KSqlDBContext(ksqlDbUrl);

      var statement = context.CreateOrReplaceTableStatement(tableName: TopicNames.ItemsTable)
        .As<Item>()
        .Where(c => c.Description != "ET")
        .GroupBy(c => c.Id)
        .Select(c => new {Id = c.Key, Count = c.Count(), });

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
  }
}