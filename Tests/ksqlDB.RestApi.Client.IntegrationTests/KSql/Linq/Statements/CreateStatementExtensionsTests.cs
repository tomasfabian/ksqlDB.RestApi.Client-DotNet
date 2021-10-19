using System.Threading.Tasks;
using FluentAssertions;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.Api.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq.Statements
{
  [TestClass]
  public class CreateStatementExtensionsTests : Infrastructure.IntegrationTests
  {
    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();
    }

    private const string StreamName = "TestStream";

    [TestMethod]
    public void CreateOrReplaceStreamStatement_ToStatementString_CalledTwiceWithSameResult()
    {
      //Arrange
      var query = Context.CreateOrReplaceStreamStatement(StreamName)
        .As<Movie>();

      //Act
      var ksql1 = query.ToStatementString();
      var ksql2 = query.ToStatementString();

      //Assert
      ksql1.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT * FROM {nameof(Movie)} EMIT CHANGES;");

      ksql1.Should().BeEquivalentTo(ksql2);
    }

    [TestMethod]
    public void CreateOrReplaceStreamStatement_ToStatementString_ComplexQueryWasGenerated()
    {
      //Arrange
      var creationMetadata = new CreationMetadata
      {
        KafkaTopic = "moviesByTitle",
        KeyFormat = SerializationFormats.Json,
        ValueFormat = SerializationFormats.Json,
        Replicas = 1,
        Partitions = 1
      };

      var query = Context.CreateOrReplaceStreamStatement(StreamName)
        .With(creationMetadata)
        .As<Movie>()
        .Where(c => c.Id < 3)
        .Select(c => new { c.Title, ReleaseYear = c.Release_Year })
        .PartitionBy(c => c.Title);

      //Act
      var ksql = query.ToStatementString();

      //Assert
      ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
 WITH ( KAFKA_TOPIC='moviesByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' )
AS SELECT Title, Release_Year AS ReleaseYear FROM Movie
WHERE Id < 3 PARTITION BY Title EMIT CHANGES;");
    }

    private const string TableName = "IntegrationTestTable";

    [TestMethod]
    public async Task CreateOrReplaceTableStatement_ExecuteStatementAsync_ResponseWasReceived()
    {
      //Arrange
      var statement = new KSqlDbStatement(StatementTemplates.DropTable(TableName));
      var response = await KSqlDbRestApiProvider.Create().ExecuteStatementAsync(statement);

      var query = Context.CreateTableStatement(TableName)
        .As<Movie>("MYMOVIESSTREAMTESTS")
        .GroupBy(c => c.Title)
        .Select(c => new { Title = c.Key, Count = c.Count() });

      //Act
      var httpResponseMessage = await query.ExecuteStatementAsync();

      //Assert
      string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
      responseContent.Should().NotBeNull();

      var responses = await httpResponseMessage.ToStatementResponsesAsync();
      responses[0].CommandStatus.Status.Should().Be("SUCCESS");
    }
  }
}