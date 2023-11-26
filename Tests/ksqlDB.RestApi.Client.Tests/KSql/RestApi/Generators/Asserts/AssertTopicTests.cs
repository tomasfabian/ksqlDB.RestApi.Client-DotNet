using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Generators.Asserts;

public class AssertTopicTests
{
  string topicName = "kafka_topic";

  [Test]
  public void CreateStatement_TopicExists()
  {
    //Arrange
    var options = new AssertTopicOptions(topicName);

    //Act
    string statement = AssertTopic.CreateStatement(exists: true, options);

    //Assert
    statement.Should().Be($@"ASSERT TOPIC {topicName};");
  }

  [Test]
  public void CreateStatement_TopicNotExists()
  {
    //Arrange
    var options = new AssertTopicOptions(topicName);

    //Act
    string statement = AssertTopic.CreateStatement(exists: false, options);

    //Assert
    statement.Should().Be($@"ASSERT NOT EXISTS TOPIC {topicName};");
  }

  [Test]
  public void CreateStatement_Timeout()
  {
    //Arrange
    var timeout = Duration.OfSeconds(10);
    
    var options = new AssertTopicOptions(topicName)
    {
      Timeout = timeout
    };

    //Act
    string statement = AssertTopic.CreateStatement(exists: true, options);

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

    var options = new AssertTopicOptions(topicName)
    {
      Properties = properties
    };

    //Act
    string statement = AssertTopic.CreateStatement(exists: true, options);

    //Assert
    statement.Should().Be($@"ASSERT TOPIC {topicName} WITH ( replicas=3, partitions=1 );");
  }

  [Test]
  public void CreateStatement_WithEmptyProperties_WithClauseWasNotGenerated()
  {
    //Arrange
    var properties = new Dictionary<string, string>();

    var options = new AssertTopicOptions(topicName)
    {
      Properties = properties
    };

    //Act
    string statement = AssertTopic.CreateStatement(exists: true, options);

    //Assert
    statement.Should().Be($@"ASSERT TOPIC {topicName};");
  }
}