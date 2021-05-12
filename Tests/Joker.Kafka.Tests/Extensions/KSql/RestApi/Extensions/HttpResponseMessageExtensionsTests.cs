using System.Threading.Tasks;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Extensions
{
  [TestClass]
  public class HttpResponseMessageExtensionsTests : KSqlDbRestApiClientTestsBase
  {
    string statement = "CREATE OR REPLACE TABLE movies";

    [TestMethod]
    public async Task ExecuteStatementAsync_HttpClientWasCalled_OkResult()
    {
      //Arrange
      var ksqlDbStatement = new KSqlDbStatement(statement);
      var restApiClient = new KSqlDbRestApiClient(HttpClientFactory);

      var httpResponseMessage = await restApiClient.ExecuteStatementAsync(ksqlDbStatement);

      //Act
      var responses = httpResponseMessage.ToStatementResponses();
      
      //Assert
      responses[0].CommandStatus.Message.Should().Be("Table created");
      responses[0].CommandStatus.Status.Should().Be("SUCCESS");
      responses[0].CommandId.Should().Be("table/`MOVIES`/create");
      responses[0].CommandSequenceNumber.Should().Be(328);
    }

    protected override string StatementResponse { get; set; } = @"[{""@type"":""currentStatus"",""statementText"":""CREATE OR REPLACE TABLE MOVIES (TITLE STRING PRIMARY KEY, ID INTEGER, RELEASE_YEAR INTEGER) WITH (KAFKA_TOPIC='Movies', KEY_FORMAT='KAFKA', PARTITIONS=1, VALUE_FORMAT='JSON');"",""commandId"":""table/`MOVIES`/create"",""commandStatus"":{""status"":""SUCCESS"",""message"":""Table created"",""queryId"":null},""commandSequenceNumber"":328,""warnings"":[]}]";
  }
}