using System.Linq.Expressions;
using System.Net;
using System.Text;
using FluentAssertions;
using ksqlDb.RestApi.Client.Infrastructure.Logging;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDb.RestApi.Client.Tests.Fakes.Logging;
using ksqlDb.RestApi.Client.Tests.Models.Movies;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi;

public class KSqlDbRestApiClientTests : KSqlDbRestApiClientTestsBase
{
  private KSqlDbRestApiClient ClassUnderTest { get; set; } = null!;
  private Mock<ILogger> LoggerMock { get; set; } = null!;
  private Mock<ILoggerFactory> LoggerFactoryMock { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    LoggerFactoryMock = new Mock<ILoggerFactory>();
    LoggerMock = new Mock<ILogger>();

    LoggerFactoryMock.Setup(c => c.CreateLogger(LoggingCategory.Name)).Returns(LoggerMock.Object);

    ClassUnderTest = new KSqlDbRestApiClient(HttpClientFactory, LoggerFactoryMock.Object);
  }

  readonly string createOrReplaceTableStatement = "CREATE OR REPLACE TABLE movies";

  private static string StatementResponse => @"[{""@type"":""currentStatus"",""statementText"":""CREATE OR REPLACE TABLE MOVIES (TITLE STRING PRIMARY KEY, ID INTEGER, RELEASE_YEAR INTEGER) WITH (KAFKA_TOPIC='Movies', KEY_FORMAT='KAFKA', PARTITIONS=1, VALUE_FORMAT='JSON');"",""commandId"":""table/`MOVIES`/create"",""commandStatus"":{""status"":""SUCCESS"",""message"":""Table created"",""queryId"":null},""commandSequenceNumber"":328,""warnings"":[]}]
";

  [Test]
  public async Task ExecuteStatementAsync_HttpClientWasCalled_OkResult()
  {
    //Arrange
    CreateHttpMocks(StatementResponse);

    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

    //Act
    var httpResponseMessage = await ClassUnderTest.ExecuteStatementAsync(ksqlDbStatement);

    //Assert
    httpResponseMessage.Should().NotBeNull();

    Mock.Get(HttpClientFactory).Verify(c => c.CreateClient(), Times.Once);
  }

  [Test]
  public async Task ExecuteStatementAsync_LogInformation()
  {
    //Arrange
    CreateHttpMocks(StatementResponse);

    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

    //Act
    var httpResponseMessage = await ClassUnderTest.ExecuteStatementAsync(ksqlDbStatement);

    //Assert
    httpResponseMessage.Should().NotBeNull();

    LoggerMock.VerifyLog(LogLevel.Debug, Times.Once);
    LoggerMock.VerifyLog(LogLevel.Information, Times.Once);
  }

  [Test]
  public void CreateHttpRequestMessage_HttpRequestMessage_WasConfigured()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

    //Act
    var httpRequestMessage = ClassUnderTest.CreateHttpRequestMessage(ksqlDbStatement);

    //Assert
    httpRequestMessage.Method.Should().Be(HttpMethod.Post);
    httpRequestMessage.RequestUri.Should().Be("/ksql");
    httpRequestMessage.Content!.Headers!.ContentType!.MediaType.Should().Be(KSqlDbRestApiClient.MediaType);
  }

  [Test]
  public async Task CreateHttpRequestMessage_HttpRequestMessage_ContentWasSet()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

    //Act
    var httpRequestMessage = ClassUnderTest.CreateHttpRequestMessage(ksqlDbStatement);

    //Assert
    var content = await httpRequestMessage.Content!.ReadAsStringAsync();
    content.Should().Be(@$"{{""ksql"":""{createOrReplaceTableStatement}"",""streamsProperties"":{{}}}}");
  }

  [Test]
  public void CreateContent_MediaTypeAndCharsetWereApplied()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

    //Act
    var stringContent = KSqlDbRestApiClient.CreateContent(ksqlDbStatement);

    //Assert
    stringContent.Headers!.ContentType!.MediaType.Should().Be(KSqlDbRestApiClient.MediaType);
    stringContent.Headers.ContentType.CharSet.Should().Be(Encoding.UTF8.WebName);
  }

  [Test]
  public void CreateContent_Encoding_OverridenCharsetWasApplied()
  {
    //Arrange
    var encoding = Encoding.Unicode;

    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement)
    {
      ContentEncoding = encoding
    };

    //Act
    var stringContent = KSqlDbRestApiClient.CreateContent(ksqlDbStatement);

    //Assert
    stringContent.Headers!.ContentType!.CharSet.Should().Be(encoding.WebName);
  }

  [Test]
  public async Task CreateContent_KSqlContentWasSet()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

    //Act
    var stringContent = KSqlDbRestApiClient.CreateContent(ksqlDbStatement);

    //Assert
    var content = await GetContent(stringContent);
      
    content.Should().Be(@$"{{""ksql"":""{createOrReplaceTableStatement}"",""streamsProperties"":{{}}}}");
  }

  [Test]
  public async Task CreateContent_CommandSequenceNumber()
  {
    //Arrange
    long commandSequenceNumber = 1000;
    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement)
    {
      CommandSequenceNumber = commandSequenceNumber
    };

    //Act
    var stringContent = KSqlDbRestApiClient.CreateContent(ksqlDbStatement);

    //Assert
    var content = await GetContent(stringContent);

    content.Should().Be(@$"{{""commandSequenceNumber"":{commandSequenceNumber},""ksql"":""{createOrReplaceTableStatement}"",""streamsProperties"":{{}}}}");
  }

  private static async Task<string> GetContent(StringContent stringContent)
  {
    var byteArray = await stringContent.ReadAsByteArrayAsync();

    var content = Encoding.Default.GetString(byteArray);

    return content;
  }

  [Test]
  public void GetEndpoint_DefaultIs_KSql()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

    //Act
    var endpoint = KSqlDbRestApiClient.GetEndpoint(ksqlDbStatement);

    //Assert
    endpoint.Should().Be("/ksql");
  }

  [Test]
  public void GetEndpoint_OverridenToQueryEndpoint()
  {
    //Arrange
    var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement)
    {
      EndpointType = EndpointType.Query
    };

    //Act
    var endpoint = KSqlDbRestApiClient.GetEndpoint(ksqlDbStatement);

    //Assert
    endpoint.Should().Be("/query");
  }

  private static string GetQueriesResponse => @"[{""@type"":""queries"",""statementText"":""SHOW QUERIES;"",""queries"":[{""queryString"":""select * from mymovies emit changes;"",""sinks"":[],""sinkKafkaTopics"":[],""id"":""_confluent-ksql-ksql-connect-clustertransient_6719152142362566835_1627490551142"",""statusCount"":{""RUNNING"":1},""queryType"":""PUSH"",""state"":""RUNNING""}],""warnings"":[]}]";

  [Test]
  public async Task GetQueriesAsync()
  {
    //Arrange
    CreateHttpMocks(GetQueriesResponse);

    //Act
    var queriesResponses = await ClassUnderTest.GetQueriesAsync();

    //Assert
    var expectedContent = GetExpectedContent(StatementTemplates.ShowQueries);

    VerifySendAsync(expectedContent);

    queriesResponses[0].StatementText.Should().Be(StatementTemplates.ShowQueries);

    queriesResponses[0].Type.Should().Be("queries");
    queriesResponses[0].Queries!.Length.Should().Be(1);
    queriesResponses[0].Queries![0].QueryType.Should().Be("PUSH");
  }

  private static string GetTopicsResponse => @"[{""@type"":""kafka_topics"",""statementText"":""SHOW TOPICS;"",""topics"":[{""name"":""AVG_SENSOR_VALUES"",""replicaInfo"":[1,1]},{""name"":""sensor_values"",""replicaInfo"":[1,1]}],""warnings"":[]}]";
  private static string GetAllTopicsResponse => @"[{""@type"":""kafka_topics"",""statementText"":""SHOW ALL TOPICS;"",""topics"":[{""name"":""AVG_SENSOR_VALUES"",""replicaInfo"":[1,1]},{""name"":""sensor_values"",""replicaInfo"":[1,1]}],""warnings"":[]}]";

  [Test]
  public async Task GetTopicsAsync()
  {
    //Arrange
    CreateHttpMocks(GetTopicsResponse);

    //Act
    var topicsResponses = await ClassUnderTest.GetTopicsAsync();

    //Assert
    var expectedContent = GetExpectedContent(StatementTemplates.ShowTopics);

    VerifySendAsync(expectedContent);

    topicsResponses[0].StatementText.Should().Be(StatementTemplates.ShowTopics);

    topicsResponses[0].Type.Should().Be("kafka_topics");
    topicsResponses[0].Topics!.Length.Should().Be(2);
  }

  [Test]
  public async Task GetAllTopicsAsync()
  {
    //Arrange
    CreateHttpMocks(GetAllTopicsResponse);

    //Act
    var topicsResponses = await ClassUnderTest.GetAllTopicsAsync();

    //Assert
    var expectedContent = GetExpectedContent(StatementTemplates.ShowAllTopics);

    VerifySendAsync(expectedContent);

    topicsResponses[0].StatementText.Should().Be(StatementTemplates.ShowAllTopics);

    topicsResponses[0].Topics!.Length.Should().Be(2);
  }

  [Test]
  public async Task GetTopicsExtendedAsync()
  {
    //Arrange
    CreateHttpMocks(GetTopicsResponse);

    //Act
    var responses = await ClassUnderTest.GetTopicsExtendedAsync();

    //Assert
    responses.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.ShowTopicsExtended);

    VerifySendAsync(expectedContent);
  }

  [Test]
  public async Task GetAllTopicsExtendedAsync()
  {
    //Arrange
    CreateHttpMocks(GetTopicsResponse);

    //Act
    var responses = await ClassUnderTest.GetAllTopicsExtendedAsync();

    //Assert
    responses.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.ShowAllTopicsExtended);
      
    VerifySendAsync(expectedContent);
  }

  readonly string queryId = "CTAS_MOVIESBYTITLE_35";
  readonly string type = "@type";

  private string TerminatePersistentQueryResponse => $@"[{{""{type}"":""currentStatus"",""statementText"":""TERMINATE {queryId};"",""commandId"":""terminate/{queryId}/execute"",""commandStatus"":{{""status"":""SUCCESS"",""message"":""Query terminated."",""queryId"":null}},""commandSequenceNumber"":40,""warnings"":[]}}]";

  [Test]
  public async Task TerminatePersistentQueryAsync()
  {
    //Arrange
    CreateHttpMocks(TerminatePersistentQueryResponse);

    //Act
    var responses = await ClassUnderTest.TerminatePersistentQueryAsync(queryId);

    //Assert
    string terminateStatement = StatementTemplates.TerminatePersistentQuery(queryId);

    var expectedContent = GetExpectedContent(terminateStatement);

    VerifySendAsync(expectedContent);

    string expectedStatement = StatementTemplates.TerminatePersistentQuery(queryId);
    responses[0].StatementText.Should().Be(expectedStatement);
  }

  //TODO: test close-query
  //https://github.com/confluentinc/ksql/issues/7559
  private static string TerminatePushQueryResponse => @"""{""@type"":""generic_error"",""error_code"":50000,""message"":""On wrong context or worker""}""";

  [Test]
  public async Task TerminatePushQueryAsync()
  {
    //Arrange
    CreateHttpMocks(TerminatePushQueryResponse);

    //Act
    var response = await ClassUnderTest.TerminatePushQueryAsync(queryId);

    //Assert
    response.IsSuccessStatusCode.Should().BeTrue();
    var closeQuery = new CloseQuery
    {
      QueryId = queryId
    };
      
    var expectedContent = await KSqlDbRestApiClient.CreateContent(closeQuery, Encoding.UTF8).ReadAsStringAsync();

    VerifySendAsync(expectedContent, "/close-query");
  }

  private static string GetExpectedContent(string statement)
  {
    string parameters = @$"{{""ksql"":""{statement}"",""streamsProperties"":{{}}}}";

    return parameters;
  }

  private void VerifySendAsync(string content, string requestUri = "/ksql")
  {
    var request = ItExpr.Is<HttpRequestMessage>(c => c.Method == HttpMethod.Post && c.RequestUri!.PathAndQuery == requestUri && c.Content!.ReadAsStringAsync().Result == content);

    HttpMessageHandlerMock.Protected()
      .Verify(nameof(HttpClient.SendAsync), Times.Once(), exactParameterMatch: true, request, ItExpr.IsAny<CancellationToken>());
  }

  private static string GetAllStreamsResponse => @"[{""@type"":""streams"",""statementText"":""SHOW STREAMS;"",""streams"":[{""type"":""STREAM"",""name"":""SENSORSSTREAM"",""topic"":""SENSORSSTREAM"",""keyFormat"":""KAFKA"",""valueFormat"":""JSON"",""isWindowed"":false},{""type"":""STREAM"",""name"":""MYMOVIESSTREAMTESTS"",""topic"":""MyMoviesStreamTest"",""keyFormat"":""JSON"",""valueFormat"":""JSON"",""isWindowed"":true}],""warnings"":[]}]";

  [Test]
  public async Task GetStreamsAsync()
  {
    //Arrange
    CreateHttpMocks(GetAllStreamsResponse);

    //Act
    var streamsResponses = await ClassUnderTest.GetStreamsAsync();

    //Assert
    var expectedContent = GetExpectedContent(StatementTemplates.ShowStreams);

    VerifySendAsync(expectedContent);

    streamsResponses[0].StatementText.Should().Be(StatementTemplates.ShowStreams);

    streamsResponses[0].Streams!.Length.Should().Be(2);
  }

  private static string GetAllTablesResponse => @"[{""@type"":""tables"",""statementText"":""SHOW TABLES;"",""tables"":[{""type"":""TABLE"",""name"":""AVG_SENSOR_VALUES"",""topic"":""AVG_SENSOR_VALUES"",""keyFormat"":""KAFKA"",""valueFormat"":""JSON"",""isWindowed"":true},{""type"":""TABLE"",""name"":""MYMOVIESTABLES"",""topic"":""MyMoviesTable"",""keyFormat"":""JSON"",""valueFormat"":""JSON"",""isWindowed"":true}],""warnings"":[]}]";

  [Test]
  public async Task GetTablesAsync()
  {
    //Arrange
    CreateHttpMocks(GetAllTablesResponse);

    //Act
    var tablesResponses = await ClassUnderTest.GetTablesAsync();

    //Assert
    var expectedContent = GetExpectedContent(StatementTemplates.ShowTables);

    VerifySendAsync(expectedContent);

    tablesResponses[0].StatementText.Should().Be(StatementTemplates.ShowTables);

    tablesResponses[0].Tables!.Length.Should().Be(2);
  }

  [Test]
  public async Task DropStreamAsync()
  {
    //Arrange
    CreateHttpMocks("[]");

    string streamName = nameof(TestStream);

    //Act
    var response = await ClassUnderTest.DropStreamAsync(streamName);

    //Assert
    response.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.DropStream(streamName));

    VerifySendAsync(expectedContent);
  }

  private class TestStream;

  [Test]
  public async Task DropStreamAsync_WithDropEntityProperties()
  {
    //Arrange
    CreateHttpMocks("[]");

    var properties = new DropFromItemProperties
    {
      UseIfExistsClause = true,
      DeleteTopic = true,
      ShouldPluralizeEntityName = false,
      IdentifierEscaping = IdentifierEscaping.Never
    };
    string streamName = $"{nameof(TestStream)}";

    //Act
    var response = await ClassUnderTest.DropStreamAsync<TestStream>(properties);

    //Assert
    response.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.DropStream(streamName, properties.UseIfExistsClause, properties.DeleteTopic));
      
    VerifySendAsync(expectedContent);
  }

  [Test]
  public async Task DropStreamAsync_IfExistsAndDeleteTopic()
  {
    //Arrange
    CreateHttpMocks("[]");

    string streamName = nameof(TestStream);
    bool useIfExistsClause = true;
    bool deleteTopic = true;

    //Act
    var response = await ClassUnderTest.DropStreamAsync(streamName, useIfExistsClause, deleteTopic);

    //Assert
    response.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.DropStream(streamName, useIfExistsClause, deleteTopic));
      
    VerifySendAsync(expectedContent);
  }

  [Test]
  public async Task DropTableAsync()
  {
    //Arrange
    CreateHttpMocks("[]");

    string tableName = nameof(TestTable);

    //Act
    var response = await ClassUnderTest.DropTableAsync(tableName);

    //Assert
    response.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.DropTable(tableName));

    VerifySendAsync(expectedContent);
  }

  [Test]
  public async Task DropTableAsync_IfExistsAndDeleteTopic()
  {
    //Arrange
    CreateHttpMocks("[]");

    string tableName = nameof(TestTable);
    bool useIfExistsClause = true;
    bool deleteTopic = true;

    //Act
    var response = await ClassUnderTest.DropTableAsync(tableName, useIfExistsClause, deleteTopic);

    //Assert
    response.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.DropTable(tableName, useIfExistsClause, deleteTopic));
      
    VerifySendAsync(expectedContent);
  }

  private class TestTable;

  [Test]
  public async Task DropTableAsync_WithDropEntityProperties()
  {
    //Arrange
    CreateHttpMocks("[]");

    var properties = new DropFromItemProperties
    {
      UseIfExistsClause = true,
      DeleteTopic = true,
      ShouldPluralizeEntityName = false
    };
    string tableName = nameof(TestTable);

    //Act
    var response = await ClassUnderTest.DropTableAsync<TestTable>(properties);

    //Assert
    response.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.DropTable(tableName, properties.UseIfExistsClause, properties.DeleteTopic));
      
    VerifySendAsync(expectedContent);
  }

  private class TestType;

  [Test]
  public async Task DropTypeAsync_WithDropEntityProperties()
  {
    //Arrange
    CreateHttpMocks("[]");

    var properties = new DropTypeProperties
    {
      ShouldPluralizeEntityName = false
    };
    string typeName = nameof(TestType);

    //Act
    var response = await ClassUnderTest.DropTypeAsync<TestType>(properties);

    //Assert
    response.Should().NotBeNull();
    var expectedContent = GetExpectedContent(StatementTemplates.DropType(typeName));

    VerifySendAsync(expectedContent);
  }

  [Test]
  public void ToInsertStatement()
  {
    //Arrange

    //Act
    var insertStatement = ClassUnderTest.ToInsertStatement(new Movie { Id = 1 });

    //Assert
    insertStatement.Sql.Should().Be("INSERT INTO Movies (Title, Id, Release_Year) VALUES (NULL, 1, 0);");
  }

  [KSqlFunction]
  public static string FormatTimestamp(long input, string format) => throw new NotSupportedException();

  [KSqlFunction]
  public static long FROM_UNIXTIME(long milliseconds) => throw new NotSupportedException();

  [KSqlFunction]
  public static long UnixTimestamp() => throw new NotSupportedException();

  private struct Article
  {
    [IgnoreByInserts]
    public long RowTime { get; set; }
    public string Title { get; set; }

    [Key]
    public int Id { get; set; }
    public string Release_Date { get; set; }
  }

  [Test]
  public void ToInsertStatement_WithFunction()
  {
    //Arrange
    Expression<Func<string>> valueExpression = () => FormatTimestamp(FROM_UNIXTIME(UnixTimestamp()), "yyyy");
    var insertValues = new InsertValues<Article>(new Article()).WithValue(c => c.Release_Date, valueExpression);

    //Act
    var insertStatement = ClassUnderTest.ToInsertStatement(insertValues);

    //Assert
    string expectedFunction = "FORMAT_TIMESTAMP(FROM_UNIXTIME(UNIX_TIMESTAMP()), 'yyyy')";
    insertStatement.Sql.Should().Be($"INSERT INTO Articles ({nameof(Article.Title)}, Id, {nameof(Article.Release_Date)}) VALUES (NULL, 0, {expectedFunction});");
  }

  [Test]
  public async Task AssertTopicExistsAsync_ReturnsTrue()
  {
    //Arrange
    string topicName = "tweetsByTitle";
    var timeout = Duration.OfSeconds(10);

    var options = new AssertTopicOptions(topicName)
    {
      Timeout = timeout
    };

    CreateHttpMocks(@"[{""@type"":""assert_topic"",""statementText"":""ASSERT TOPIC tweetsByTitle TIMEOUT 3 SECONDS;"",""topicName"":""tweetsByTitle"",""exists"":true,""warnings"":[]}]");

    //Act
    var response = await ClassUnderTest.AssertTopicExistsAsync(options);

    //Assert
    response[0].TopicName.Should().Be(topicName);
    response[0].Exists.Should().BeTrue();
  }

  [Test]
  public async Task AssertTopicExistsAsync_ReturnsFalse()
  {
    //Arrange
    string topicName = "tweetsByTitleX";
    var timeout = Duration.OfSeconds(10);

    var options = new AssertTopicOptions(topicName)
    {
      Timeout = timeout
    };

    CreateHttpMocks(@"[{""@type"":""generic_error"",""error_code"":41700,""message"":""Topic tweetsByTitleX does not exist""}]");

    //Act
    var response = await ClassUnderTest.AssertTopicExistsAsync(options);

    //Assert
    response[0].Exists.Should().BeFalse();
  }

  [Test]
  public async Task AssertSchemaExistsAsync_ReturnsTrue()
  {
    //Arrange
    string subject = "Kafka-key";
    var timeout = Duration.OfSeconds(10);

    var options = new AssertSchemaOptions(subject)
    {
      Timeout = timeout
    };

    CreateHttpMocks(@"[{""@type"":""assert_schema"",""statementText"":""ASSERT SCHEMA SUBJECT 'Kafka-key' TIMEOUT 3 SECONDS;"",""subject"":""Kafka-key"",""id"":null,""exists"":true,""warnings"":[]}]  ");

    //Act
    var response = await ClassUnderTest.AssertSchemaExistsAsync(options);

    //Assert
    response[0].Subject.Should().Be(subject);
    response[0].Exists.Should().BeTrue();
  }

  [Test]
  public async Task AssertSchemaExistsAsync_ReturnsFalse()
  {
    //Arrange
    string subject = "Kafka-key";
    var timeout = Duration.OfSeconds(10);

    var options = new AssertSchemaOptions(subject)
    {
      Timeout = timeout
    };

    CreateHttpMocks(@"[{""@type"":""generic_error"",""error_code"":41700,""message"":""Schema with subject name Kafka-key exists""}]");

    //Act
    var response = await ClassUnderTest.AssertSchemaExistsAsync(options);

    //Assert
    response[0].Exists.Should().BeFalse();
  }

  [Test]
  public async Task CreateSourceTableAsync()
  {
    //Arrange
    CreateHttpMocks(@"[{""@type"":""tables""}]");

    var creationMetadata = new EntityCreationMetadata("moviesByTitle", 1)
    {
      Replicas = 1,
      Partitions = 1,
      ShouldPluralizeEntityName = true,
    };

    //Act
    var response = await ClassUnderTest.CreateSourceTableAsync<Movie>(creationMetadata);

    //Assert
    var expectedContent = GetExpectedContent(@"CREATE SOURCE TABLE Movies (\r\n\tTitle VARCHAR,\r\n\tId INT PRIMARY KEY,\r\n\tRelease_Year INT\r\n) WITH ( KAFKA_TOPIC=\u0027moviesByTitle\u0027, VALUE_FORMAT=\u0027Json\u0027, PARTITIONS=\u00271\u0027, REPLICAS=\u00271\u0027 );".ReplaceLineEndings());

    VerifySendAsync(expectedContent);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Test]
  public async Task CreateSourceStreamAsync()
  {
    //Arrange
    CreateHttpMocks(@"[{""@type"":""streams""}]");

    var creationMetadata = new EntityCreationMetadata("moviesByTitle", 1)
    {
      Replicas = 1,
      Partitions = 1,
      ShouldPluralizeEntityName = true,
    };

    //Act
    var response = await ClassUnderTest.CreateSourceStreamAsync<Movie>(creationMetadata);

    //Assert
    var expectedContent = GetExpectedContent(@"CREATE SOURCE STREAM Movies (\r\n\tTitle VARCHAR,\r\n\tId INT KEY,\r\n\tRelease_Year INT\r\n) WITH ( KAFKA_TOPIC=\u0027moviesByTitle\u0027, VALUE_FORMAT=\u0027Json\u0027, PARTITIONS=\u00271\u0027, REPLICAS=\u00271\u0027 );".ReplaceLineEndings());

    VerifySendAsync(expectedContent);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }
}
