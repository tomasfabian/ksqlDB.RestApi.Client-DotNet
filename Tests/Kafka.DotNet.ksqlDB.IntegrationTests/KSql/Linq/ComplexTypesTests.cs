﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.Linq;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq
{
  [TestClass]
  public class ComplexTypesTests
  {
    private IKSqlDbRestApiClient restApiClient;
    protected KSqlDBContext Context;

    [TestInitialize]
    public void Initialize()
    {
      var ksqlDbUrl = @"http:\\localhost:8088";

      var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

      restApiClient = new KSqlDbRestApiClient(httpClientFactory);

      var contextOptions = new KSqlDBContextOptions(KSqlDbRestApiProvider.KsqlDbUrl)
      {
        ShouldPluralizeFromItemName = true
      };

      Context = new KSqlDBContext(contextOptions);
    }

    [TestMethod]
    public async Task ReceiveArrayWithComplexElements()
    {
      //Arrange
      var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
Drop type EventCategory;
Drop table Events;
"));

      //Act
      httpResponseMessage = await restApiClient.CreateTypeAsync<EventCategory>();
      httpResponseMessage = await restApiClient.CreateTableAsync<Event>(new EntityCreationMetadata() { KafkaTopic = "Events", Partitions = 1 });

      var eventCategory = new EventCategory()
      {
        Name = "xyz"
      };
      var testEvent = new Event
      {
        Id = 1,
        Places = new[] { "Place1", "Place2" },
        Categories = new[] { eventCategory, new EventCategory { Name = "puk" } }
      };

      var semaphoreSlim = new SemaphoreSlim(0, 1);

      var receivedValues = new List<Event>();
      var subscription = Context.CreateQueryStream<Event>().Take(1)
        .Subscribe(value =>
          {
            receivedValues.Add(value);
          }, error =>
          {
            semaphoreSlim.Release();
          },
          () =>
          {
            semaphoreSlim.Release();
          });

      //httpResponseMessage = await restApiClient.InsertIntoAsync(testEvent);//TODO: insert arrays and complex types
      httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
INSERT INTO Events (Id, Places, Categories) VALUES (1, ARRAY['1','2','3'], ARRAY[STRUCT(Name := 'kuko'), STRUCT(Name := 'puk')]);"));

      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

      //Assert
      await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(5));

      receivedValues.Count.Should().Be(1);
      receivedValues[0].Places.Length.Should().Be(3);
      receivedValues[0].Categories.ToArray()[0].Name.Should().Be("kuko");
      receivedValues[0].Categories.ToArray()[1].Name.Should().Be("puk");

      using (subscription) { }
    }

    record Event
    {
      [Key]
      public int Id { get; set; }

      public string[] Places { get; init; }
      //public EventCategory[] Categories { get; init; }
      public IEnumerable<EventCategory> Categories { get; init; }
    }

    record EventCategory
    {
      public string Name { get; init; }
    }
  }
}