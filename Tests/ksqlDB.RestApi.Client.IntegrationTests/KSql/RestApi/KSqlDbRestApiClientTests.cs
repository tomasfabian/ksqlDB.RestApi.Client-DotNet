using System.Net;
using System.Reactive.Concurrency;
using System.Text.Json;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.Http;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;

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

  private string SuccessStatus => "SUCCESS";

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

    responseObject?[0].CommandStatus.Status.Should().Be(SuccessStatus);
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
    var httpResponseMessage = await restApiClient.CreateOrReplaceStreamAsync<MyMoviesStreamTest>(metadata);
    var c = await httpResponseMessage.ToStatementResponsesAsync().ConfigureAwait(false);

    //Assert
    httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
    var responseObject = JsonSerializer.Deserialize<StatementResponse[]>(responseContent);

    responseObject?[0].CommandStatus.Status.Should().Be("SUCCESS");
    responseObject?[0].CommandStatus.Message.Should().Be("Stream created");
  }

  [TestMethod]
  public async Task CreateTypeAsync()
  {
    //Arrange
    var httpResponseMessage = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(@"
Drop type Person;
Drop type Address;
"));
      
    //Act
    httpResponseMessage = await restApiClient.CreateTypeAsync<Address>();
    httpResponseMessage = await restApiClient.CreateTypeAsync<Person>();

    //Assert
    httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  public record Address
  {
    public int Number { get; set; }
    public string Street { get; set; }
  }

  public class Person
  {
    public string Name { get; set; }
    public Address Address { get; set; }
  }

  #region Connectors

  private string SinkConnectorName => "mock-sink-connector";

  [TestMethod]
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

  [TestMethod]
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
    connectorsResponse[0].Connectors.Any(c => c.Name == SourceConnectorName).Should().BeTrue();
    connectorsResponse[0].Type.Should().Be("connector_list");

    httpResponseMessage = await restApiClient.DropConnectorAsync($"`{SourceConnectorName}`");

    httpResponseMessage = await restApiClient.DropConnectorAsync("UnknownConnector");
    var content1 = await httpResponseMessage.ToStatementResponsesAsync();

    httpResponseMessage = await restApiClient.DropConnectorIfExistsAsync("UnknownConnector");
    var content2 = await httpResponseMessage.ToStatementResponsesAsync();

    connectorsResponse = await restApiClient.GetConnectorsAsync();
    connectorsResponse[0].Connectors.Any(c => c.Name == SourceConnectorName).Should().BeFalse();
  }

  #endregion

  [TestMethod]
  public async Task GetStreamsAsync()
  {
    //Arrange

    //Act
    var streamResponses = await restApiClient.GetStreamsAsync();

    //Assert
    streamResponses[0].StatementText.Should().Be(StatementTemplates.ShowStreams);
    streamResponses[0].Streams.Select(c => c.Name).Contains(nameof(MyMoviesStreamTest).ToUpper() + "S").Should().BeTrue();
  }

  [TestMethod]
  public async Task GetTablesAsync()
  {
    //Arrange

    //Act
    var tablesResponses = await restApiClient.GetTablesAsync();

    //Assert
    tablesResponses[0].StatementText.Should().Be(StatementTemplates.ShowTables);

    tablesResponses[0].Tables.Select(c => c.Name).Contains(nameof(MyMoviesTable).ToUpper() + "S").Should().BeTrue();
  }

  [TestMethod]
  public async Task GetAllTopicsAsync()
  {
    //Arrange

    //Act
    var topicsResponses = await restApiClient.GetAllTopicsAsync();

    //Assert
    topicsResponses[0].StatementText.Should().Be(StatementTemplates.ShowAllTopics);
    topicsResponses[0].Topics.Select(c => c.Name).Contains(nameof(MyMoviesTable)).Should().BeTrue();
  }

  [TestMethod]
  public async Task TerminatePersistentQueryAsync()
  {
    //Arrange
    string topicName = "testTableAsSelect";

    var contextOptions = new KSqlDBContextOptions(@"http:\\localhost:8088")
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

    var query = queries.SelectMany(c => c.Queries).FirstOrDefault(c => c.SinkKafkaTopics.Contains(topicName));

    //Act
    statementResponse = await restApiClient.TerminatePersistentQueryAsync(query.Id);

    //Assert
    query.QueryType.Should().Be("PERSISTENT");
    query.SinkKafkaTopics.Should().Contain(topicName);

    response.IsSuccessStatusCode.Should().BeTrue();

    statementResponse[0].CommandStatus.Status.Should().Be(SuccessStatus);
    statementResponse[0].CommandStatus.Message.Should().Be("Query terminated.");
  }

  [TestMethod]
  public async Task TerminatePushQueryAsync()
  {
    //Arrange
    var contextOptions = new KSqlDBContextOptions(@"http:\\localhost:8088");
    await using var context = new KSqlDBContext(contextOptions);
      
    var subscription = await context
      .CreateQueryStream<MyMoviesTable>()
      .SubscribeOn(ThreadPoolScheduler.Instance)
      .SubscribeAsync(_ => {}, e => { }, () => { });
      
    int queriesCount = (await restApiClient.GetQueriesAsync()).SelectMany(c => c.Queries).Count();

    //Act
    var response = await restApiClient.TerminatePushQueryAsync(subscription.QueryId);

    //Assert
      
    //https://github.com/confluentinc/ksql/issues/7559
    //"{"@type":"generic_error","error_code":50000,"message":"On wrong context or worker"}"
    //response.IsSuccessStatusCode.Should().BeTrue()

    await Task.Delay(TimeSpan.FromSeconds(7));
    var queriesResponses = await restApiClient.GetQueriesAsync();
    queriesResponses.SelectMany(c => c.Queries).Count().Should().Be(queriesCount - 1);
  }

  [TestMethod]
  public async Task GetQueriesAsync()
  {
    //Arrange

    //Act
    var queriesResponses = await restApiClient.GetQueriesAsync();

    //Assert
    Console.WriteLine(string.Join(',', queriesResponses[0].Queries.Select(c => c.Id)));
    queriesResponses[0].StatementText.Should().Be(StatementTemplates.ShowQueries);
  }

  [TestMethod]
  public async Task DropConnectorIfExistsAsync_DoesNotExist_WarningResponse()
  {
    //Arrange

    //Act
    var httpResponseMessage = await restApiClient.DropConnectorIfExistsAsync("UnknownConnector");
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    content[0].Type.Should().Be("warning_entity");
  }

  [TestMethod]
  public async Task DropConnectorAsync_DoesNotExist_ErrorResponse()
  {
    //Arrange

    //Act
    var httpResponseMessage = await restApiClient.DropConnectorAsync("UnknownConnector");
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    content[0].Type.Should().Contain("error");
  }

  record Movie2
  {
    [Key]
    public int Id { get; set; }
    public string Title { get; init; }
    public Lead_Actor Actor { get; set; }
  }

  [TestMethod]
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

  private string CreateStreamStatement(string streamName = "TestTable")
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

  [TestMethod]
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

  [TestMethod]
  public async Task DropTableAsync()
  {
    //Arrange
    string tableName = Guid.NewGuid().ToString().Substring(0, 8);
    var response = await restApiClient.ExecuteStatementAsync(new KSqlDbStatement(CreateTableStatement(tableName)));

    //Act
    var httpResponseMessage = await restApiClient.DropTableAsync(tableName, true, true);
    var content = await httpResponseMessage.ToStatementResponsesAsync();

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
  }

  internal record MyMoviesTable : MyMoviesStreamTest
  {

  }

  internal record MyMoviesStreamTest
  {
    [Key]
    public int Id { get; set; }

    public string Title { get; set; }

    public string Timestamp { get; set; }

    public int Release_Year { get; set; }

    public int[] NumberOfDays { get; set; }

    public IDictionary<string, int> Dictionary { get; set; }
    public Dictionary<string, int> Dictionary2 { get; set; }

    //#pragma warning disable CS0649
    public int DontFindMe;
    //#pragma warning restore CS0649

    public int DontFindMe2 { get; }
  }
    
  [TestMethod]
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
    content[0].CommandStatus?.Status.Should().BeOneOf(default(string), SuccessStatus);
  }

  record IoTSensor
  {
    [Key]
    public string SensorId { get; set; }
    public int Value { get; set; }
  }

  [TestMethod]
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
    content[0].CommandStatus?.Status.Should().BeOneOf(default(string), SuccessStatus);
  }

  [TestMethod]
  public async Task SessionVariables()
  {
    //Arrange
    string typeName = "FromSessionVar";
    var _ = await restApiClient.DropTypeAsync(typeName);

    var statement = new KSqlDbStatement("CREATE TYPE ${typeName} AS STRUCT<name VARCHAR, address ADDRESS>;")
    {
      SessionVariables = { { "typeName", typeName } }
    };

    //Act
    var httpResponseMessage = await restApiClient.ExecuteStatementAsync(statement);

    //Assert
    httpResponseMessage.IsSuccessStatusCode.Should().BeTrue();

    var content = await httpResponseMessage.ToStatementResponsesAsync();
    content[0].CommandStatus?.Status.Should().BeOneOf(default(string), SuccessStatus);
  }
}
