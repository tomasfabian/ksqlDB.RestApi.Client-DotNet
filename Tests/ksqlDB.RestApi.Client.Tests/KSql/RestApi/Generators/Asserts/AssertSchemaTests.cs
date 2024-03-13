using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Generators.Asserts;

public class AssertSchemaTests
{
  private readonly string subjectName = "Kafka-key";

  [Test]
  public void CreateStatement_SubjectExists()
  {
    //Arrange
    var options = new AssertSchemaOptions(subjectName);

    //Act
    string statement = AssertSchema.CreateStatement(exists: true, options);

    //Assert
    statement.Should().Be($"ASSERT SCHEMA SUBJECT '{subjectName}';");
  }

  private readonly int schemaId = 21;

  [Test]
  public void CreateStatement_SubjectIdExists()
  {
    //Arrange
    var options = new AssertSchemaOptions("", schemaId);

    //Act
    string statement = AssertSchema.CreateStatement(exists: true, options);

    //Assert
    statement.Should().Be($"ASSERT SCHEMA ID {schemaId};");
  }

  [Test]
  public void CreateStatement_SubjectIdAndNameExists()
  {
    //Arrange
    var options = new AssertSchemaOptions(subjectName, schemaId);

    //Act
    string statement = AssertSchema.CreateStatement(exists: true, options);

    //Assert
    statement.Should().Be($"ASSERT SCHEMA SUBJECT '{subjectName}' ID {schemaId};");
  }

  [Test]
  public void CreateStatement_Timeout()
  {
    //Arrange
    var timeout = Duration.OfSeconds(10);
    var options = new AssertSchemaOptions(subjectName)
    {
      Timeout = timeout
    };

    //Act
    string statement = AssertSchema.CreateStatement(exists: true, options);

    //Assert
    statement.Should().Be($"ASSERT SCHEMA SUBJECT '{subjectName}' TIMEOUT {timeout.TotalSeconds.Value} SECONDS;");
  }
}
