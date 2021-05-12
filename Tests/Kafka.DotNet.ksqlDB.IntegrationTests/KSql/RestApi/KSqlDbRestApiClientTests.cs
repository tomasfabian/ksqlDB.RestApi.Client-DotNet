using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Enums;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Serialization;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi
{
  [TestClass]
  public class KSqlDbRestApiClientTests
  {
    private IKSqlDbRestApiClient restApiClient;

    [TestInitialize]
    public void Initialize()
    {
      var ksqlDbUrl = @"http:\\localhost:8088";

      var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

      restApiClient = new KSqlDbRestApiClient(httpClientFactory);
    }

    [TestMethod]
    public async Task ExecuteStatementAsync()
    {
      //Arrange
      KSqlDbStatement ksqlDbStatement = new(CreateTableStatement());

      //Act
      var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement);

      //Assert
      httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
      var responseObject = JsonSerializer.Deserialize<StatementResponse[]>(responseContent);

      responseObject?[0].CommandStatus.Status.Should().Be("SUCCESS");
      responseObject?[0].CommandStatus.Message.Should().Be("Table created");
    }

    [TestMethod]
    [ExpectedException(typeof(TaskCanceledException))]
    public async Task ExecuteStatementAsync_Cancelled_ThrowsTaskCanceledException()
    {
      //Arrange
      KSqlDbStatement ksqlDbStatement = new(CreateTableStatement());
      var cts = new CancellationTokenSource();

      //Act
      var httpResponseMessageTask = restApiClient.ExecuteStatementAsync(ksqlDbStatement, cts.Token);
      cts.Cancel();

      HttpResponseMessage httpResponseMessage = await httpResponseMessageTask;
    }
    
    private string CreateTableStatement(string tableName = "TestTable")
    {
      return $@"CREATE OR REPLACE TABLE {tableName} (
        title VARCHAR PRIMARY KEY,
        id INT,
        release_year INT
      ) WITH (
        KAFKA_TOPIC='{tableName}',
        PARTITIONS=1,
        VALUE_FORMAT = 'JSON'
      );";
    }

    [TestMethod]
    public async Task CreateTable()
    {
      //Arrange
      var metadata = GetEntityCreationMetadata(nameof(MyMoviesTable));

      //Act
      var httpResponseMessage = await restApiClient.CreateTable<MyMoviesTable>(metadata, ifNotExists: true);

      //Assert
      httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
      var responseObject = JsonSerializer.Deserialize<StatementResponse[]>(responseContent);
    }

    private static EntityCreationMetadata GetEntityCreationMetadata(string topicName)
    {
      EntityCreationMetadata metadata = new()
      {
        KeyFormat = SerializationFormats.Json,
        KafkaTopic = topicName,
        Partitions = 1,
        Replicas = 1,
        WindowType = WindowType.Tumbling,
        WindowSize = "10 SECONDS",
        Timestamp = nameof(MyMoviesTable.Timestamp),
        TimestampFormat = "yyyy-MM-dd''T''HH:mm:ssX"
      };

      return metadata;
    }

    [TestMethod]
    public async Task CreateOrReplaceStream()
    {
      //Arrange
      var metadata = GetEntityCreationMetadata(nameof(MyMoviesStreamTest));

      //Act
      var httpResponseMessage = await restApiClient.CreateOrReplaceStream<MyMoviesStreamTest>(metadata);

      //Assert
      httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
      var responseObject = JsonSerializer.Deserialize<StatementResponse[]>(responseContent);

      responseObject?[0].CommandStatus.Status.Should().Be("SUCCESS");
      responseObject?[0].CommandStatus.Message.Should().Be("Stream created");
    }

    internal record MyMoviesTable : MyMoviesStreamTest
    {

    }

    internal record MyMoviesStreamTest
    {
      [ksqlDB.KSql.RestApi.Statements.Annotations.Key]
      public int Id { get; set; }

      public string Title { get; set; }

      public string Timestamp { get; set; }

      public int Release_Year { get; set; }

      public int[] NumberOfDays { get; set; }

      public IDictionary<string, int> Dictionary { get; set; }
      public Dictionary<string, int> Dictionary2 { get; set; }

      public int DontFindMe;

      public int DontFindMe2 { get; }
    }
  }
}