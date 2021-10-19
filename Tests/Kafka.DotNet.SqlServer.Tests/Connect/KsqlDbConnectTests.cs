using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using FluentAssertions;
using InsideOut.Consumer;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Connectors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.Connector.Cdc;
using SqlServer.Connector.Cdc.Connectors;
using SqlServer.Connector.Connect;
using SqlServer.Connector.Tests.Data;
using UnitTests;
using ConfigurationProvider = SqlServer.Connector.Tests.Config.ConfigurationProvider;

namespace SqlServer.Connector.Tests.Connect
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
      
      await ksqlDbConnect.DropConnectorIfExistsAsync(connectorName);
    }

    private static string exposedBootstrapServers = "localhost:29092";

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
    
    static readonly IoTSensor Sensor = new() {SensorId = "1-X", Value = 42};

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
      
      ApplicationDbContext.Sensors.Add(Sensor);
      var saveResult = await ApplicationDbContext.SaveChangesAsync();

      ApplicationDbContext.Entry(Sensor).State = EntityState.Detached;

      var updatedSensor = Sensor with {Value = 43};

      ApplicationDbContext.Sensors.Update(updatedSensor);
      saveResult = await ApplicationDbContext.SaveChangesAsync();

      ApplicationDbContext.Sensors.Remove(updatedSensor);
      saveResult = await ApplicationDbContext.SaveChangesAsync();

      //Act
      string topicName = $"{databaseServerName}.dbo.{tableName}";
      
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
      var createOperation = messages[0];

      createOperation.OperationType.Should().Be(ChangeDataCaptureType.Created);
      createOperation.Before.Should().BeNull();
      createOperation.After.Should().NotBeNull();
      createOperation.After.Should().Be(Sensor);
      
      var updateOperation = messages[1];

      updateOperation.OperationType.Should().Be(ChangeDataCaptureType.Updated);
      updateOperation.Before.Should().Be(Sensor);
      updateOperation.After.Should().Be(Sensor with { Value = 43 });
      
      var deleteOperation = messages[2];

      deleteOperation.OperationType.Should().Be(ChangeDataCaptureType.Deleted);
      deleteOperation.Before.Should().Be(Sensor with { Value = 43 });
      deleteOperation.After.Should().BeNull();
    }

    [TestMethod]
    public async Task GetConnectorsAsync()
    {
      var ksqlDbUrl = Configuration["ksqlDb:Url"];

      var ksqlDbConnect = new KsqlDbConnect(new Uri(ksqlDbUrl));
      
      var response = await ksqlDbConnect.GetConnectorsAsync();

      response.IsSuccessStatusCode.Should().BeTrue();

      var connectors = await response.Content.ReadAsStringAsync();
      var connectorsResponse = JsonSerializer.Deserialize<ConnectorsResponse[]>(connectors);

      connectorsResponse[0].Connectors.Select(c => c.Name.ToLower()).Contains(connectorName).Should().BeTrue();
      var testConnector = connectorsResponse[0].Connectors.FirstOrDefault(c => c.Name.ToLower() == connectorName);

      testConnector.State.Should().Contain("RUNNING");
    }

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
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }
  }
}