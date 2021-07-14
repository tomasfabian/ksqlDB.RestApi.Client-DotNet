﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FluentAssertions;
using Kafka.DotNet.InsideOut.Consumer;
using Kafka.DotNet.SqlServer.Cdc;
using Kafka.DotNet.SqlServer.Cdc.Connectors;
using Kafka.DotNet.SqlServer.Connect;
using Kafka.DotNet.SqlServer.Tests.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;
using ConfigurationProvider = Kafka.DotNet.SqlServer.Tests.Config.ConfigurationProvider;

namespace Kafka.DotNet.SqlServer.Tests.Connect
{
  [TestClass]
  [TestCategory("Integration")]
  public class KsqlDbConnectTests : TestBase<KsqlDbConnect>
  {
    private static ApplicationDbContext ApplicationDbContext { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
      ApplicationDbContext = new ApplicationDbContext();

      await DropDependenciesAsync(ApplicationDbContext.Database);
      
      await ApplicationDbContext.Database.MigrateAsync();

      string connectionString = Configuration.GetConnectionString("DefaultConnection");

      var cdcClient = new CdcClient(connectionString);
      await cdcClient.CdcEnableDbAsync();
      await cdcClient.CdcEnableTableAsync("Sensors");
    }

    private static readonly IConfiguration Configuration = ConfigurationProvider.CreateConfiguration();
    private static readonly string connectorName = "test_connector";
    
    [ClassCleanup]
    public static async Task ClassCleanup()
    {
      await DropDependenciesAsync(ApplicationDbContext.Database);

      ApplicationDbContext = null;
    }

    private static async Task DropConnectorAsync()
    {
      var ksqlDbUrl = Configuration["ksqlDb:Url"];

      var ksqlDbConnect = new KsqlDbConnect(new Uri(ksqlDbUrl));

      await ksqlDbConnect.DropConnectorAsync(connectorName);
    }

    private static string exposedBootstrapServers = "localhost:29092";

    private static async Task DeleteTopicAsync(string topicName)
    {
      try
      {
        var config = new AdminClientConfig
        {
          BootstrapServers = exposedBootstrapServers
        };

        var adminClientBuilder = new AdminClientBuilder(config);

        var adminClient = adminClientBuilder.Build();

        await adminClient.DeleteTopicsAsync(new[] {topicName});
        
        // await Task.Delay(5000);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    private static async Task DropDependenciesAsync(DatabaseFacade databaseFacade)
    {
      await DropConnectorAsync();
      //await DeleteTopicAsync(cdcTopicName);

      await ApplicationDbContext.Database.EnsureDeletedAsync();
    }
    
    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      var ksqlDbUrl = Configuration["ksqlDb:Url"];

      ClassUnderTest = new KsqlDbConnect(new Uri(ksqlDbUrl));
    }

    private static string tableName = "Sensors";
    private static string cdcTopicName = "sqlserver2019Tests.dbo.Sensors";

    [TestMethod]
    public async Task CreateConnectorAsync_AndReceivesDatabaseChangeObjects()
    {
      //Arrange
      var connectionString = Configuration.GetConnectionString("DefaultConnection");

      var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
      {
        DataSource = "sqlserver2019"
      };

      connectionString = connectionStringBuilder.ConnectionString;

      var bootstrapServers = Configuration["Kafka:BootstrapServers"];
      string databaseServerName = "sqlserver2019Tests";

      var connectorMetadata = new SqlServerConnectorMetadata(connectionString)
        .SetTableIncludeListPropertyName($"dbo.{tableName}")
        .SetJsonKeyConverter()
        .SetJsonValueConverter()
        .SetProperty("database.history.kafka.bootstrap.servers", bootstrapServers)
        .SetProperty("database.history.kafka.topic", $"dbhistory.test.{tableName}")
        .SetProperty("database.server.name", databaseServerName)
        //.SetProperty("topic.creation.enable", "true")
        .SetProperty("key.converter.schemas.enable", "false")
        .SetProperty("value.converter.schemas.enable", "false")
        .SetProperty("include.schema.changes", "false") as SqlServerConnectorMetadata;

      //Act
      var httpResponseMessage = await ClassUnderTest.CreateConnectorAsync(connectorName, connectorMetadata);

      //Assert
      //var statementResponse = httpResponseMessage.ToStatementResponse();
      httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();

      await ReceivesDatabaseChangeObjectsAsync(databaseServerName);
    }

    private static async Task ReceivesDatabaseChangeObjectsAsync(string databaseServerName)
    {
      string uniqueGroup = Guid.NewGuid().ToString()[..8];

      var consumerConfig = new ConsumerConfig
      {
        BootstrapServers = exposedBootstrapServers,
        GroupId = $"Client-01-{uniqueGroup}",
        AutoOffsetReset = AutoOffsetReset.Earliest
      };

      short expectedItemsCount = 3;
      IList<DatabaseChangeObject<IoTSensor>> receivedSensors = new List<DatabaseChangeObject<IoTSensor>>();
      
      //await Task.Delay(5000);
      var sensor = new IoTSensor {SensorId = "1-X", Value = 42};
      ApplicationDbContext.Sensors.Add(sensor);
      var saveResult = await ApplicationDbContext.SaveChangesAsync();

      sensor.Value = 43;
      ApplicationDbContext.Sensors.Update(sensor);
      saveResult = await ApplicationDbContext.SaveChangesAsync();

      ApplicationDbContext.Sensors.Remove(sensor);
      saveResult = await ApplicationDbContext.SaveChangesAsync();

      //await Task.Delay(5000);

      //Act
      string topicName = $"{databaseServerName}.dbo.{tableName}";
      //topicName = "sqlserver2019.dbo.Sensors";
      var kafkaConsumer =
        new KafkaConsumer<string, DatabaseChangeObject<IoTSensor>>(topicName, consumerConfig);

      var topicAsyncEnumerable = kafkaConsumer.ConnectToTopic()
        .ToAsyncEnumerable()
        .Where(c => c.Message.Value != null && c.Message.Value.Op != "r")
        .Take(expectedItemsCount);

      await foreach (var consumeResult in topicAsyncEnumerable)
      {
        Console.WriteLine(consumeResult.Message);
        receivedSensors.Add(consumeResult.Message.Value);
      }

      //Assert
      receivedSensors.Count.Should().Be(expectedItemsCount);
      VerifyMessages(receivedSensors.ToArray());

      using (kafkaConsumer)
      {
      }
    }

    private static void VerifyMessages(DatabaseChangeObject<IoTSensor>[] messages)
    {

    }
  }
}