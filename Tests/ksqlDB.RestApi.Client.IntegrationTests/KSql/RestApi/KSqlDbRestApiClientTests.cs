using System.Net;
using System.Reactive.Concurrency;
using System.Text.Json;
using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.Http;
using ksqlDb.RestApi.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;

public class KSqlDbRestApiClientTests
{
  private KSqlDbRestApiClient restApiClient = null!;

  [SetUp]
  public void Initialize()
  {
    var ksqlDbUrl = Helpers.TestConfig.KSqlDbUrl;

    var httpClientFactory = new HttpClientFactory(new Uri(ksqlDbUrl));

    restApiClient = new KSqlDbRestApiClient(httpClientFactory);
  }
  
  [Test]
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

    responseObject?[0].CommandStatus!.Status.Should().Be(CommandStatus.Success);
    responseObject?[0].CommandStatus!.Message.Should().Be("Table created");
  }

  [Test]
  public void ExecuteStatementAsync_Cancelled_ThrowsTaskCanceledException()
  {
    //Arrange
    KSqlDbStatement ksqlDbStatement = new(CreateTableStatement());
    var cts = new CancellationTokenSource();

    NUnit.Framework.Assert.ThrowsAsync<TaskCanceledException>(() =>
    {
      //Act
      var httpResponseMessageTask = restApiClient.ExecuteStatementAsync(ksqlDbStatement, cts.Token);
      cts.Cancel();

      return httpResponseMessageTask;
    });
  }

  private static string CreateTableStatement(string tableName = "TestTable")
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

  [Test]
  public async Task CreateTable()
  {
    //Arrange
    var metadata = GetEntityCreationMetadata(nameof(MyMoviesTable));

    //Act
    var httpResponseMessage = await restApiClient.CreateTableAsync<MyMoviesTable>(metadata, ifNotExists: true);

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
      TimestampFormat = "yyyy-MM-dd''T''HH:mm:ssX",
      ShouldPluralizeEntityName = false
    };

    return metadata;
  }

  internal record MyMoviesTableTest : MyMoviesStreamTest;

  [Test]
  public async Task CreateOrReplaceStream()
  {
    //Arrange
    var metadata = GetEntityCreationMetadata(nameof(MyMoviesTableTest));
    await restApiClient.DropStreamAsync(nameof(MyMoviesTableTest));

    //Act
    var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<MyMoviesTableTest>(metadata);
    var statementResponses = await httpResponseMessage.ToStatementResponsesAsync().ConfigureAwait(false);

    //Assert
    httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
    var responseObject = JsonSerializer.Deserialize<StatementResponse[]>(responseContent);

    responseObject?[0].CommandStatus!.Status.Should().Be(CommandStatus.Success);
    responseObject?[0].CommandStatus!.Message.Should().Be("Stream created");
  }

  [Test]
  public async Task CreateTypeAsync()
  {
    //Arrange
    await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@$"
Drop type {nameof(Person)};
Drop type {nameof(Address)};
"));

    //Act
    var httpResponseMessage = await restApiClient.CreateTypeAsync<Address>();
    httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    httpResponseMessage = await restApiClient.CreateTypeAsync<Person>();
    httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

    //Assert
    httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  public record Address
  {
    public int Number { get; set; }
    public string Street { get; set; } = null!;
  }

  public class Person
  {
    public string Name { get; set; } = null!;
    public Address Address { get; set; } = null!;
  }

  #region Connectors

  private static string SinkConnectorName => "mock-sink-connector";

  [Test]
  public async Task CreateSinkConnectorAsync()
  {
    //Arrange
    var connectorConfig = new Dictionary<string, string> {
      { "connector.class", "org.apache.kafka.connect.tools.MockSinkConnector" },
      { "topics.regex", "mock-sink*"},
    };

    //Act
    var httpResponseMessage = await restApiClient.CreateSinkConnectorAsync(connectorConfig, SinkConnectorName);

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();

    //Cleanup
    httpResponseMessage = await restApiClient.DropConnectorAsync($"`{SinkConnectorName}`");
  }

  private string SourceConnectorName => "mock-source-connector";

  [Test]
  public async Task CreateSourceConnectorAsync_And_GetConnectorAsync_And_DropConnectorIfExistsAsync()
  {
    //Arrange
    var connectorConfig = new Dictionary<string, string> {
      { "connector.class", "org.apache.kafka.connect.tools.MockSourceConnector" },
    };

    //Act
    var httpResponseMessage = await restApiClient.CreateSourceConnectorAsync(connectorConfig, SourceConnectorName);

    var connectorsResponse = await restApiClient.GetConnectorsAsync();

    //Assert
    connectorsResponse[0].Connectors.Should().NotBeNull();
    connectorsResponse[0].Connectors!.Any(c => c.Name == SourceConnectorName).Should().BeTrue();
    connectorsResponse[0].Type.Should().Be("connector_list");

    httpResponseMessage = await restApiClient.DropConnectorAsync($"`{SourceConnectorName}`");

    httpResponseMessage = await restApiClient.DropConnectorAsync("UnknownConnector");
    var content1 = await httpResponseMessage.ToStatementResponsesAsync();

    httpResponseMessage = await restApiClient.DropConnectorIfExistsAsync("UnknownConnector");
    var content2 = await httpResponseMessage.ToStatementResponsesAsync();

    connectorsResponse = await restApiClient.GetConnectorsAsync();
    connectorsResponse[0].Connectors!.Any(c => c.Name == SourceConnectorName).Should().BeFalse();
  }

  #endregion

  [Test]
  public async Task GetStreamsAsync()
  {
    //Arrange

    //Act
    var streamResponses = await restApiClient.GetStreamsAsync();

    //Assert
    streamResponses[0].StatementText.Should().Be(StatementTemplates.ShowStreams);
    streamResponses[0].Streams!.Select(c => c.Name).Contains(nameof(MyMoviesStreamTest).ToUpper() + "S").Should().BeTrue();
  }

  [Test]
  public async Task GetTablesAsync()
  {
    //Arrange

    //Act
    var tablesResponses = await restApiClient.GetTablesAsync();

    //Assert
    tablesResponses[0].StatementText.Should().Be(StatementTemplates.ShowTables);

    tablesResponses[0].Tables!.Select(c => c.Name).Contains(nameof(MyMoviesTable).ToUpper()).Should().BeTrue();
  }

  [Test]
  public async Task GetAllTopicsAsync()
  {
    //Arrange

    //Act
    var topicsResponses = await restApiClient.GetAllTopicsAsync();

    //Assert
    topicsResponses[0].StatementText.Should().Be(StatementTemplates.ShowAllTopics);
    topicsResponses[0].Topics!.Select(c => c.Name).Contains(nameof(MyMoviesTable)).Should().BeTrue();
  }

  [Test]
  public async Task TerminatePersistentQueryAsync()
  {
    //Arrange
    string topicName = "testTableAsSelect";

    var contextOptions = new KSqlDBContextOptions(Helpers.TestConfig.KSqlDbUrl)
    {
      ShouldPluralizeFromItemName = false
    };
    await using var context = new KSqlDBContext(contextOptions);

    string createTableAsSelectStatement = context.CreateOrReplaceTableStatement("TestTableAsSelect")
      .With(new CreationMetadata
      {
        KafkaTopic = topicName,
        KeyFormat = SerializationFormats.Json,
        ValueFormat = SerializationFormats.Json
      })
      .As<object>("TestTable")
      .ToStatementString();

    var statement = new KSqlDbStatement(createTableAsSelectStatement);

    var response = await restApiClient.ExecuteStatementAsync(statement);
    var statementResponse = await response.ToStatementResponsesAsync();

    var queries = await restApiClient.GetQueriesAsync();

    var query = queries.SelectMany(c => c.Queries!).First(c => c.SinkKafkaTopics!.Contains(topicName));

    //Act
    statementResponse = await restApiClient.TerminatePersistentQueryAsync(query.Id!);

    //Assert
    query.QueryType.Should().Be(QueryType.Persistent);
    query.SinkKafkaTopics.Should().Contain(topicName);

    response.IsSuccessStatusCode.Should().BeTrue();

    statementResponse[0].CommandStatus!.Status.Should().BeOneOf(CommandStatus.Executing, CommandStatus.Success);
    statementResponse[0].CommandStatus!.Message.Should().BeOneOf("Executing statement", "Query terminated.");
  }

  [Test]
  public async Task TerminatePushQueryAsync()
  {
    //Arrange
    var contextOptions = new KSqlDBContextOptions(Helpers.TestConfig.KSqlDbUrl)
    {
      ShouldPluralizeFromItemName = false
    };
    await using var context = new KSqlDBContext(contextOptions);
    var cts = new CancellationTokenSource();
      
    var subscription = await context
      .CreatePushQuery<MyMoviesTable>()
      .SubscribeOn(ThreadPoolScheduler.Instance)
      .SubscribeAsync(_ => {}, e => { }, () => { }, cts.Token);
      
    int queriesCount = await GetPersistentQueriesCountAsync(QueryType.Push, cts.Token);

    //Act
    var response = await restApiClient.TerminatePushQueryAsync(subscription.QueryId!, cts.Token);

    //Assert
    await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
    (await GetPersistentQueriesCountAsync(QueryType.Push, cts.Token)).Should().Be(queriesCount - 1);
    await cts.CancelAsync();
  }

  private async Task<int> GetPersistentQueriesCountAsync(string queryType, CancellationToken token)
  {
    return (await restApiClient.GetQueriesAsync(token)).SelectMany(c => c.Queries!).Count(c => c.QueryType == queryType);
  }

  [Test]
  public async Task GetQueriesAsync()
  {
    //Arrange

    //Act
    var queriesResponses = await restApiClient.GetQueriesAsync();

    //Assert
    Console.WriteLine(string.Join(',', queriesResponses[0].Queries!.Select(c => c.Id)));
    queriesResponses[0].StatementText.Should().Be(StatementTemplates.ShowQueries);
  }

  [Test]
  public async Task DropConnectorIfExistsAsync_DoesNotExist_WarningResponse()
  {
    //Arrange

    //Act
    var httpResponseMessage = await restApiClient.DropConnectorIfExistsAsync("UnknownConnector");
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    content[0].Type.Should().Be("warning_entity");
  }

  [Test]
  public async Task DropConnectorAsync_DoesNotExist_ErrorResponse()
  {
    //Arrange

    //Act
    var httpResponseMessage = await restApiClient.DropConnectorAsync("UnknownConnector");
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    content[0].Type.Should().Contain("error");
  }

  private record Movie2
  {
    [Key]
    public int Id { get; set; }
    public string Title { get; init; } = null!;
    public Lead_Actor? Actor { get; set; } 
  }

  [Test]
  public async Task InsertIntoAsync_NullValue()
  {
    //Arrange
    var response = await restApiClient.CreateTypeAsync<Lead_Actor>();
    response = await restApiClient.CreateTableAsync<Movie2>(new EntityCreationMetadata() { EntityName = "puk", KafkaTopic = "puk", Partitions = 1 });

    var movie2 = new Movie2
    {
      Id = 1,
      Actor = null
    };

    //Act
    var httpResponseMessage = await restApiClient.InsertIntoAsync(movie2, new InsertProperties { EntityName = "puk" });

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
  }

  private static string CreateStreamStatement(string streamName = "TestTable")
  {
    return $@"CREATE OR REPLACE STREAM {streamName} (
        title VARCHAR KEY,
        id INT,
        release_year INT
      ) WITH (
        KAFKA_TOPIC='{streamName}',
        PARTITIONS=1,
        VALUE_FORMAT = 'JSON'
      );";
  }

  [Test]
  public async Task DropStreamAsync()
  {
    //Arrange
    string streamName = Guid.NewGuid().ToString().Substring(0, 8);
    var response = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(CreateStreamStatement(streamName)));

    //Act
    var httpResponseMessage = await restApiClient.DropStreamAsync(streamName, true, true);
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
  }

  [Test]
  public async Task DropTableAsync()
  {
    //Arrange
    string tableName = Guid.NewGuid().ToString()[..8];
    var response = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(CreateTableStatement(tableName)));

    //Act
    var httpResponseMessage = await restApiClient.DropTableAsync(tableName, true, true);
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
  }

  internal record MyMoviesTable : MyMoviesStreamTest;

  internal record MyMoviesStreamTest
  {
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Timestamp { get; set; } = null!;

    public int ReleaseYear { get; set; }

    public int[] NumberOfDays { get; set; } = null!;

    public IDictionary<string, int> Dictionary { get; set; } = null!;
    public Dictionary<string, int> Dictionary2 { get; set; } = null!;

    //#pragma warning disable CS0649
    public int DoNotFindMe;
    //#pragma warning restore CS0649

    public int DontFindMe2 { get; }
  }
    
  [Test]
  public async Task CreateSourceStream()
  {
    //Arrange
    string entityName = "Test_Source_Stream";

    var metadata = new EntityCreationMetadata(entityName, 1)
    {
      EntityName = entityName
    };

    //Act
    var httpResponseMessage = await restApiClient.CreateSourceStreamAsync<Movie>(metadata, ifNotExists: true);
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
    content[0].CommandStatus?.Status.Should().BeOneOf(default(string), CommandStatus.Success);
  }

  private record IoTSensor
  {
    [Key]
    public string SensorId { get; set; } = null!;
    public int Value { get; set; }
  }

  [Test]
  public async Task CreateSourceTable()
  {
    //Arrange
    string entityName = "Test_Source_Table";

    var metadata = new EntityCreationMetadata(entityName, 1)
    {
      EntityName = entityName
    };

    //Act
    var httpResponseMessage = await restApiClient.CreateSourceTableAsync<IoTSensor>(metadata, ifNotExists: true);
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
    content[0].CommandStatus?.Status.Should().BeOneOf(default(string), CommandStatus.Success);
  }

  [Test]
  public async Task SessionVariables()
  {
    //Arrange
    string typeName = "FromSessionVar";
    var _ = await restApiClient.DropTypeAsync(typeName);

    var statement = new KSqlDbStatement("CREATE TYPE ${typeName} AS STRUCT<name VARCHAR, address ADDRESS>;")
    {
      SessionVariables = new Dictionary<string, object> { { "typeName", typeName } }
    };

    //Act
    var httpResponseMessage = await restApiClient.ExecuteStatementAsync(statement);

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();

    var content = await httpResponseMessage.ToStatementResponsesAsync();
    content[0].CommandStatus?.Status.Should().BeOneOf(default(string), CommandStatus.Success);
  }
}
