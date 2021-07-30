using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
    
    string statement = "CREATE OR REPLACE TABLE movies";

    private string StatementResponse => @"[{""@type"":""currentStatus"",""statementText"":""CREATE OR REPLACE TABLE MOVIES (TITLE STRING PRIMARY KEY, ID INTEGER, RELEASE_YEAR INTEGER) WITH (KAFKA_TOPIC='Movies', KEY_FORMAT='KAFKA', PARTITIONS=1, VALUE_FORMAT='JSON');"",""commandId"":""table/`MOVIES`/create"",""commandStatus"":{""status"":""SUCCESS"",""message"":""Table created"",""queryId"":null},""commandSequenceNumber"":328,""warnings"":[]}]
";

    [TestMethod]
    public async Task ExecuteStatementAsync_HttpClientWasCalled_OkResult()
    {
      //Arrange
      CreateHttpMocks(StatementResponse);

      var ksqlDbStatement = new KSqlDbStatement(statement);

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
      var ksqlDbStatement = new KSqlDbStatement(statement);

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
      var ksqlDbStatement = new KSqlDbStatement(statement);

      //Act
      var httpRequestMessage = ClassUnderTest.CreateHttpRequestMessage(ksqlDbStatement);

      //Assert
      var content = await httpRequestMessage.Content.ReadAsStringAsync();
      content.Should().Be(@$"{{""ksql"":""{statement}"",""streamsProperties"":{{}}}}");
    }

    [TestMethod]
    public void CreateContent_MediaTypeAndCharsetWereApplied()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(statement);

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

      var ksqlDbStatement = new KSqlDbStatement(statement)
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
      var ksqlDbStatement = new KSqlDbStatement(statement);

      //Act
      var stringContent = ClassUnderTest.CreateContent(ksqlDbStatement);

      //Assert
      var content = await GetContent(stringContent);
      
      content.Should().Be(@$"{{""ksql"":""{statement}"",""streamsProperties"":{{}}}}");
    }

    [TestMethod]
    public async Task CreateContent_CommandSequenceNumber()
    {
      //Arrange
      long commandSequenceNumber = 1000;
      var ksqlDbStatement = new KSqlDbStatement(statement)
      {
        CommandSequenceNumber = commandSequenceNumber
      };

      //Act
      var stringContent = ClassUnderTest.CreateContent(ksqlDbStatement);

      //Assert
      var content = await GetContent(stringContent);

      content.Should().Be(@$"{{""commandSequenceNumber"":{commandSequenceNumber},""ksql"":""{statement}"",""streamsProperties"":{{}}}}");
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
      var ksqlDbStatement = new KSqlDbStatement(statement);

      //Act
      var endpoint = KSqlDbRestApiClient.GetEndpoint(ksqlDbStatement);

      //Assert
      endpoint.Should().Be("/ksql");
    }

    [TestMethod]
    public void GetEndpoint_OverridenToQueryEndpoint()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(statement)
      {
        EndpointType = EndpointType.Query
      };

      //Act
      var endpoint = KSqlDbRestApiClient.GetEndpoint(ksqlDbStatement);

      //Assert
      endpoint.Should().Be("/query");
    }

    private string GetTopicsResponse => @"[{""@type"":""kafka_topics"",""statementText"":""SHOW TOPICS;"",""topics"":[{""name"":""AVG_SENSOR_VALUES"",""replicaInfo"":[1,1]},{""name"":""sensor_values"",""replicaInfo"":[1,1]}],""warnings"":[]}]";
    private string GetAllTopicsResponse => @"[{""@type"":""kafka_topics"",""statementText"":""SHOW ALL TOPICS;"",""topics"":[{""name"":""AVG_SENSOR_VALUES"",""replicaInfo"":[1,1]},{""name"":""sensor_values"",""replicaInfo"":[1,1]}],""warnings"":[]}]";

    [TestMethod]
    public async Task GetTopicsAsync()
    {
      //Arrange
      CreateHttpMocks(GetTopicsResponse);

      //Act
      var queriesResponses = await ClassUnderTest.GetTopicsAsync();

      //Assert
      queriesResponses[0].StatementText.Should().Be("SHOW TOPICS;");

      queriesResponses[0].Type.Should().Be("kafka_topics");
      queriesResponses[0].Topics.Length.Should().Be(2);
    }

    [TestMethod]
    public async Task GetAllTopicsAsync()
    {
      //Arrange
      CreateHttpMocks(GetAllTopicsResponse);

      //Act
      var queriesResponses = await ClassUnderTest.GetAllTopicsAsync();

      //Assert
      queriesResponses[0].StatementText.Should().Be("SHOW ALL TOPICS;");

      queriesResponses[0].Topics.Length.Should().Be(2);
    }
  }
}