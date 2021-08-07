using System;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;
using Moq.Protected;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi
{
  [TestClass]
  public class KSqlDbRestApiClientTests : KSqlDbRestApiClientTestsBase
  {
    private KSqlDbRestApiClient ClassUnderTest { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = new KSqlDbRestApiClient(HttpClientFactory);
    }
    
    string createOrReplaceTableStatement = "CREATE OR REPLACE TABLE movies";

    private string StatementResponse => @"[{""@type"":""currentStatus"",""statementText"":""CREATE OR REPLACE TABLE MOVIES (TITLE STRING PRIMARY KEY, ID INTEGER, RELEASE_YEAR INTEGER) WITH (KAFKA_TOPIC='Movies', KEY_FORMAT='KAFKA', PARTITIONS=1, VALUE_FORMAT='JSON');"",""commandId"":""table/`MOVIES`/create"",""commandStatus"":{""status"":""SUCCESS"",""message"":""Table created"",""queryId"":null},""commandSequenceNumber"":328,""warnings"":[]}]
";

    [TestMethod]
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
    
    [TestMethod]
    public void CreateHttpRequestMessage_HttpRequestMessage_WasConfigured()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

      //Act
      var httpRequestMessage = ClassUnderTest.CreateHttpRequestMessage(ksqlDbStatement);

      //Assert
      httpRequestMessage.Method.Should().Be(HttpMethod.Post);
      httpRequestMessage.RequestUri.Should().Be("/ksql");
      httpRequestMessage.Content.Headers.ContentType.MediaType.Should().Be(KSqlDbRestApiClient.MediaType);
    }
    
    [TestMethod]
    public async Task CreateHttpRequestMessage_HttpRequestMessage_ContentWasSet()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

      //Act
      var httpRequestMessage = ClassUnderTest.CreateHttpRequestMessage(ksqlDbStatement);

      //Assert
      var content = await httpRequestMessage.Content.ReadAsStringAsync();
      content.Should().Be(@$"{{""ksql"":""{createOrReplaceTableStatement}"",""streamsProperties"":{{}}}}");
    }

    [TestMethod]
    public void CreateContent_MediaTypeAndCharsetWereApplied()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

      //Act
      var stringContent = ClassUnderTest.CreateContent(ksqlDbStatement);

      //Assert
      stringContent.Headers.ContentType.MediaType.Should().Be(KSqlDbRestApiClient.MediaType);
      stringContent.Headers.ContentType.CharSet.Should().Be(Encoding.UTF8.WebName);
    }

    [TestMethod]
    public void CreateContent_Encoding_OverridenCharsetWasApplied()
    {
      //Arrange
      var encoding = Encoding.Unicode;

      var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement)
      {
        ContentEncoding = encoding
      };

      //Act
      var stringContent = ClassUnderTest.CreateContent(ksqlDbStatement);

      //Assert
      stringContent.Headers.ContentType.CharSet.Should().Be(encoding.WebName);
    }

    [TestMethod]
    public async Task CreateContent_KSqlContentWasSet()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

      //Act
      var stringContent = ClassUnderTest.CreateContent(ksqlDbStatement);

      //Assert
      var content = await GetContent(stringContent);
      
      content.Should().Be(@$"{{""ksql"":""{createOrReplaceTableStatement}"",""streamsProperties"":{{}}}}");
    }

    [TestMethod]
    public async Task CreateContent_CommandSequenceNumber()
    {
      //Arrange
      long commandSequenceNumber = 1000;
      var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement)
      {
        CommandSequenceNumber = commandSequenceNumber
      };

      //Act
      var stringContent = ClassUnderTest.CreateContent(ksqlDbStatement);

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

    [TestMethod]
    public void GetEndpoint_DefaultIs_KSql()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(createOrReplaceTableStatement);

      //Act
      var endpoint = KSqlDbRestApiClient.GetEndpoint(ksqlDbStatement);

      //Assert
      endpoint.Should().Be("/ksql");
    }

    [TestMethod]
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

    private string GetQueriesResponse => @"[{""@type"":""queries"",""statementText"":""SHOW QUERIES;"",""queries"":[{""queryString"":""select * from mymovies emit changes;"",""sinks"":[],""sinkKafkaTopics"":[],""id"":""_confluent-ksql-ksql-connect-clustertransient_6719152142362566835_1627490551142"",""statusCount"":{""RUNNING"":1},""queryType"":""PUSH"",""state"":""RUNNING""}],""warnings"":[]}]";

    [TestMethod]
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
      queriesResponses[0].Queries.Length.Should().Be(1);
      queriesResponses[0].Queries[0].QueryType.Should().Be("PUSH");
    }

    private string GetTopicsResponse => @"[{""@type"":""kafka_topics"",""statementText"":""SHOW TOPICS;"",""topics"":[{""name"":""AVG_SENSOR_VALUES"",""replicaInfo"":[1,1]},{""name"":""sensor_values"",""replicaInfo"":[1,1]}],""warnings"":[]}]";
    private string GetAllTopicsResponse => @"[{""@type"":""kafka_topics"",""statementText"":""SHOW ALL TOPICS;"",""topics"":[{""name"":""AVG_SENSOR_VALUES"",""replicaInfo"":[1,1]},{""name"":""sensor_values"",""replicaInfo"":[1,1]}],""warnings"":[]}]";

    [TestMethod]
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
      topicsResponses[0].Topics.Length.Should().Be(2);
    }

    [TestMethod]
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

      topicsResponses[0].Topics.Length.Should().Be(2);
    }

    [TestMethod]
    public async Task GetTopicsExtendedAsync()
    {
      //Arrange
      CreateHttpMocks(GetTopicsResponse);

      //Act
      var responses = await ClassUnderTest.GetTopicsExtendedAsync();

      //Assert
      var expectedContent = GetExpectedContent(StatementTemplates.ShowTopicsExtended);
      
      VerifySendAsync(expectedContent);
    }

    [TestMethod]
    public async Task GetAllTopicsExtendedAsync()
    {
      //Arrange
      CreateHttpMocks(GetTopicsResponse);

      //Act
      var responses = await ClassUnderTest.GetAllTopicsExtendedAsync();

      //Assert
      var expectedContent = GetExpectedContent(StatementTemplates.ShowAllTopicsExtended);
      
      VerifySendAsync(expectedContent);
    }
    
    string queryId = "CTAS_MOVIESBYTITLE_35";
    string type = "@type";

    private string TerminatePersistentQueryResponse => $@"[{{""{type}"":""currentStatus"",""statementText"":""TERMINATE {queryId};"",""commandId"":""terminate/{queryId}/execute"",""commandStatus"":{{""status"":""SUCCESS"",""message"":""Query terminated."",""queryId"":null}},""commandSequenceNumber"":40,""warnings"":[]}}]";

    [TestMethod]
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
    private string TerminatePushQueryResponse => @"""{""@type"":""generic_error"",""error_code"":50000,""message"":""On wrong context or worker""}""";

    [TestMethod]
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

    private string GetExpectedContent(string statement)
    {
      string parameters = @$"{{""ksql"":""{statement}"",""streamsProperties"":{{}}}}";

      return parameters;
    }

    private void VerifySendAsync(string content, string requestUri = @"/ksql")
    {
      var request = ItExpr.Is<HttpRequestMessage>(c => c.Method == HttpMethod.Post && c.RequestUri.PathAndQuery == requestUri && c.Content.ReadAsStringAsync().Result == content);
      
      httpMessageHandlerMock.Protected()
        .Verify(nameof(HttpClient.SendAsync), Times.Once(), exactParameterMatch: true, request, ItExpr.IsAny<CancellationToken>());
    }

    private string GetAllStreamsResponse => @"[{""@type"":""streams"",""statementText"":""SHOW STREAMS;"",""streams"":[{""type"":""STREAM"",""name"":""SENSORSSTREAM"",""topic"":""SENSORSSTREAM"",""keyFormat"":""KAFKA"",""valueFormat"":""JSON"",""isWindowed"":false},{""type"":""STREAM"",""name"":""MYMOVIESSTREAMTESTS"",""topic"":""MyMoviesStreamTest"",""keyFormat"":""JSON"",""valueFormat"":""JSON"",""isWindowed"":true}],""warnings"":[]}]";

    [TestMethod]
    public async Task GetStreamsAsync()
    {
      //Arrange
      CreateHttpMocks(GetAllStreamsResponse);

      //Act
      var queriesResponses = await ClassUnderTest.GetStreamsAsync();

      //Assert
      var expectedContent = GetExpectedContent(StatementTemplates.ShowStreams);
      
      VerifySendAsync(expectedContent);

      queriesResponses[0].StatementText.Should().Be(StatementTemplates.ShowStreams);

      queriesResponses[0].Streams.Length.Should().Be(2);
    }

    private string GetAllTablesResponse => @"[{""@type"":""tables"",""statementText"":""SHOW TABLES;"",""tables"":[{""type"":""TABLE"",""name"":""AVG_SENSOR_VALUES"",""topic"":""AVG_SENSOR_VALUES"",""keyFormat"":""KAFKA"",""valueFormat"":""JSON"",""isWindowed"":true},{""type"":""TABLE"",""name"":""MYMOVIESTABLES"",""topic"":""MyMoviesTable"",""keyFormat"":""JSON"",""valueFormat"":""JSON"",""isWindowed"":true}],""warnings"":[]}]";

    [TestMethod]
    public async Task GetTablesAsync()
    {
      //Arrange
      CreateHttpMocks(GetAllTablesResponse);

      //Act
      var queriesResponses = await ClassUnderTest.GetTablesAsync();

      //Assert
      var expectedContent = GetExpectedContent(StatementTemplates.ShowTables);
      
      VerifySendAsync(expectedContent);

      queriesResponses[0].StatementText.Should().Be(StatementTemplates.ShowTables);

      queriesResponses[0].Tables.Length.Should().Be(2);
    }
  }
}