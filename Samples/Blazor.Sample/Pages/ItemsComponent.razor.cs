using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Sample.Data;
using Kafka.DotNet.ksqlDB.InsideOut.Consumer;
using Microsoft.AspNetCore.Components;

namespace Blazor.Sample.Pages
{
  partial class ItemsComponent
  {
    [Inject]
    private IKafkaConsumer<int, Item> ItemsConsumer { get; init; }

    protected override Task OnInitializedAsync()
    {
      ItemsConsumer.ConnectToTopicAsync().Subscribe(c =>
      {

      });

      return base.OnInitializedAsync();
    }
  }
}