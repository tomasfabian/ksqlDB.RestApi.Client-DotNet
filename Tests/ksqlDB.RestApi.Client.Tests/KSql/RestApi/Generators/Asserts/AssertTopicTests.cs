using FluentAssertions;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Generators.Asserts;

public class AssertTopicTests
{
  string topicName = "kafka_topic";

  [Test]
  public void CreateStatement_TopicExists()
  {
    //Arrange

    //Act
    string statement = AssertTopic.CreateStatement(exists:true, topicName);

    //Assert
    statement.Should().Be($@"ASSERT TOPIC {topicName};");
  }

  [Test]
  public void CreateStatement_TopicNotExists()
  {
    //Arrange

    //Act
    string statement = AssertTopic.CreateStatement(exists:false, topicName);

    //Assert
    statement.Should().Be($@"ASSERT NOT EXISTS TOPIC {topicName};");
  }

  [Test]
  public void CreateStatement_Timeout()
  {
    //Arrange
    var timeout = Duration.OfSeconds(10);

    //Act
    string statement = AssertTopic.CreateStatement(exists: true, topicName, properties: null, timeout);

    //Assert
    statement.Should().Be($@"ASSERT TOPIC {topicName} TIMEOUT {timeout.TotalSeconds.Value} SECONDS;");
  }

  [Test]
  public void CreateStatement_WithProperties()
  {
    //Arrange
    var properties = new Dictionary<string, string>
    {
      { "replicas", "3" },
      { "partitions", "1" },
    };

    //Act
    string statement = AssertTopic.CreateStatement(exists: true, topicName, properties);

    //Assert
    statement.Should().Be($@"ASSERT TOPIC {topicName} WITH ( replicas=3, partitions=1 );");
  }

  [Test]
  public void CreateStatement_WithEmptyProperties_WithClauseWasNotGenerated()
  {
    //Arrange
    var properties = new Dictionary<string, string>();

    //Act
    string statement = AssertTopic.CreateStatement(exists: true, topicName, properties);

    //Assert
    statement.Should().Be($@"ASSERT TOPIC {topicName};");
  }
}