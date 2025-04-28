using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDb.RestApi.Client.IntegrationTests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq.Statements;

public class CreateStatementExtensionsTests : Infrastructure.IntegrationTests
{
  private const string StreamName = "TestStreamStatement";

  [Test]
  public void CreateOrReplaceStreamStatement_ToStatementString_CalledTwiceWithSameResult()
  {
    //Arrange
    var query = Context.CreateOrReplaceStreamStatement(StreamName)
      .As<Movie>();

    //Act
    var ksql1 = query.ToStatementString();
    var ksql2 = query.ToStatementString();

    //Assert
    ksql1.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName} AS 
SELECT * FROM {nameof(Movie)} EMIT CHANGES;");

    ksql1.Should().BeEquivalentTo(ksql2);
  }

  private const string StreamEntityName = "MYMOVIESSTREAMTESTS";

  [Test]
  public async Task CreateOrReplaceStreamStatement_ToStatementString_ComplexQueryWasGenerated()
  {
    //Arrange
    var restApiClient = KSqlDbRestApiProvider.Create();

    var statement = new KSqlDbStatement(StatementTemplates.DropStream(StreamName));
    var response = await restApiClient.ExecuteStatementAsync(statement);

    EntityCreationMetadata metadata = new()
    {
      EntityName = StreamEntityName,
      KafkaTopic = nameof(Movie)+"Test2",
      Partitions = 1,
      Replicas = 1
    };

    var httpResponseMessage = await restApiClient.CreateStreamAsync<Movie>(metadata, ifNotExists: false);

    var creationMetadata = new CreationMetadata
    {
      KafkaTopic = "moviesByTitle",
      KeyFormat = SerializationFormats.Json,
      ValueFormat = SerializationFormats.Json,
      Replicas = 1,
      Partitions = 1
    };

    var createStatement = Context.CreateOrReplaceStreamStatement(StreamName)
      .With(creationMetadata)
      .As<Movie>(StreamEntityName)
      .Where(c => c.Id < 3)
      .Select(c => new { c.Id, c.Title, ReleaseYear = c.Release_Year })
      .PartitionBy(c => c.Id);

    //Act
    var ksql = createStatement.ToStatementString();
    httpResponseMessage = await createStatement.ExecuteStatementAsync();

    //Assert
    ksql.ReplaceLineEndings().Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
 WITH ( KAFKA_TOPIC='moviesByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' ) AS 
SELECT Id, Title, Release_Year AS ReleaseYear FROM {StreamEntityName}
 WHERE Id < 3 PARTITION BY Id EMIT CHANGES;".ReplaceLineEndings());

    var responses = await httpResponseMessage.ToStatementResponsesAsync();
    responses[0].CommandStatus!.Status.Should().BeOneOf("SUCCESS", "EXECUTING");
  }

  private const string TableName = "IntegrationTestTable";
  private const string EntityName = "movies_test112";

  [Test]
  public async Task CreateOrReplaceTableStatement_ExecuteStatementAsync_ResponseWasReceived()
  {
    //Arrange
    var restApiClient = KSqlDbRestApiProvider.Create();

    await restApiClient.CreateStreamAsync<Movie>(new EntityCreationMetadata(EntityName, 1) { EntityName = EntityName, ShouldPluralizeEntityName = false });

    var statement = new KSqlDbStatement(StatementTemplates.DropTable(TableName));
    var response = await restApiClient.ExecuteStatementAsync(statement);

    int retryCount = 0;
    while ((await KSqlDbRestApiProvider.Create().GetTablesAsync()).SelectMany(c => c.Tables!).Any(c => c.Name == TableName.ToUpper()))
    {
      if(retryCount++ > 5)
        return;

      await Task.Delay(TimeSpan.FromSeconds(1));
    }

    var createStatement = Context.CreateTableStatement(TableName)
      .As<Movie>(EntityName)
      .GroupBy(c => c.Title)
      .Select(c => new { Title = c.Key, Count = c.Count() });

    //Act
    var httpResponseMessage = await createStatement.ExecuteStatementAsync();

    //Assert
    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
    responseContent.Should().NotBeNull();

    var responses = await httpResponseMessage.ToStatementResponsesAsync();

    responses[0].CommandStatus!.Status.Should().Be("SUCCESS");
  }
}
