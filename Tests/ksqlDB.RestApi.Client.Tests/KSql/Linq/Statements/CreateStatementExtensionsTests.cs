using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.Api.Client.Tests.Models;
using ksqlDB.Api.Client.Tests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDB.RestApi.Client.KSql.RestApi.Serialization;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.KSql.Linq.Statements;

[TestClass]
public class CreateStatementExtensionsTests : TestBase
{
  private TestableDbProvider DbProvider { get; set; } = null!;

  protected virtual string StatementResponse { get; set; } = @"[{""@type"":""currentStatus"", ""commandSequenceNumber"":2174,""warnings"":[]}]";

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    DbProvider = new TestableDbProvider(TestParameters.KsqlDBUrl, StatementResponse);
  }

  private const string StreamName = "TestStream";

  [TestMethod]
  public void CreateOrReplaceStreamStatement_ToStatementString_CalledTwiceWithSameResult()
  {
    //Arrange
    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .As<Location>();

    //Act
    var ksql1 = query.ToStatementString();
    var ksql2 = query.ToStatementString();

    //Assert
    ksql1.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT * FROM {nameof(Location)}s EMIT CHANGES;");

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

    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .With(creationMetadata)
      .As<Movie>()
      .Where(c => c.Id < 3)
      .Select(c => new {c.Title, ReleaseYear = c.Release_Year})
      .PartitionBy(c => c.Title);

    //Act
    var ksql = query.ToStatementString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
 WITH ( KAFKA_TOPIC='moviesByTitle', KEY_FORMAT='Json', VALUE_FORMAT='Json', PARTITIONS='1', REPLICAS='1' )
AS SELECT Title, Release_Year AS ReleaseYear FROM Movies
WHERE Id < 3 PARTITION BY Title EMIT CHANGES;");
  }

  private const string TableName = "TestTable";

  [TestMethod]
  public async Task CreateOrReplaceTableStatement_ExecuteStatementAsync_ResponseWasReceived()
  {
    //Arrange
    var query = DbProvider.CreateOrReplaceTableStatement(TableName)
      .As<Location>();

    //Act
    var httpResponseMessage = await query.ExecuteStatementAsync();

    //Assert
    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
    responseContent.Should().BeEquivalentTo(StatementResponse);
  }

  [TestMethod]
  public void GroupByHaving()
  {
    //Arrange
    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .As<Movie>()
      .GroupBy(c => c.Title)
      .Having(c => c.Count() > 2);

    //Act
    var ksql = query.ToStatementString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT * FROM Movies GROUP BY Title HAVING Count(*) > 2 EMIT CHANGES;");
  }

  [TestMethod]
  public void Take()
  {
    //Arrange
    int limit = 3;

    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .As<Movie>()
      .Take(limit);

    //Act
    var ksql = query.ToStatementString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT * FROM Movies EMIT CHANGES LIMIT {limit};");
  }

  [TestMethod]
  public void WindowedBy()
  {
    //Arrange
    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .As<Movie>()
      .GroupBy(c => new { c.Title })
      .WindowedBy(new TimeWindows(Duration.OfMinutes(2)));

    //Act
    var ksql = query.ToStatementString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT * FROM Movies WINDOW TUMBLING (SIZE 2 MINUTES) GROUP BY Title EMIT CHANGES;");
  }

  [TestMethod]
  public void Join()
  {
    //Arrange
    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .As<Movie>()        
      .Join(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title, ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToStatementString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT movie.Title Title, actor.Actor_Name AS ActorName FROM Movies movie
INNER JOIN Actors actor
ON movie.Title = actor.Title
EMIT CHANGES;");
  }

  [TestMethod]
  public void FullOuterJoin()
  {
    //Arrange
    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .As<Movie>()        
      .FullOuterJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title, ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToStatementString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT movie.Title Title, actor.Actor_Name AS ActorName FROM Movies movie
FULL OUTER JOIN Actors actor
ON movie.Title = actor.Title
EMIT CHANGES;");
  }

  [TestMethod]
  public void LeftJoin()
  {
    //Arrange
    var query = DbProvider.CreateOrReplaceStreamStatement(StreamName)
      .As<Movie>()        
      .LeftJoin(
        Source.Of<Lead_Actor>("Actors"),
        movie => movie.Title,
        actor => actor.Title,
        (movie, actor) => new
        {
          Title = movie.Title, ActorName = actor.Actor_Name
        }
      );

    //Act
    var ksql = query.ToStatementString();

    //Assert
    ksql.Should().BeEquivalentTo(@$"CREATE OR REPLACE STREAM {StreamName}
AS SELECT movie.Title Title, actor.Actor_Name AS ActorName FROM Movies movie
LEFT JOIN Actors actor
ON movie.Title = actor.Title
EMIT CHANGES;");
  }
}
