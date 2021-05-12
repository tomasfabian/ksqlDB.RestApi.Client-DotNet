using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;
using Kafka.DotNet.ksqlDB.Tests.Models.Movies;
using NUnit.Framework;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi.Statements
{
  public class CreateInsertTests
  {
    [Test]
    public void Generate()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title"};

      //Act
      string statement = new CreateInsert().Generate(movie, null);

      //Assert
      statement.Should().Be(@"INSERT INTO Movies (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    }

    [Test]
    public void Generate_OverrideEntityName()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title"};
      var insertProperties = new InsertProperties
      {
        EntityName = "TestName"
      };

      //Act
      string statement = new CreateInsert().Generate(movie, insertProperties);

      //Assert
      statement.Should().Be(@$"INSERT INTO {insertProperties.EntityName}s (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    }

    [Test]
    public void Generate_OverrideEntityName_ShouldNotPluralize()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title"};
      var insertProperties = new InsertProperties
      {
        EntityName = "TestName",
        ShouldPluralizeEntityName = false
      };

      //Act
      string statement = new CreateInsert().Generate(movie, insertProperties);

      //Assert
      statement.Should().Be(@$"INSERT INTO {insertProperties.EntityName} (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    }

    [Test]
    public void Generate_ShouldNotPluralizeEntityName()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title"};
      var insertProperties = new InsertProperties
      {
        ShouldPluralizeEntityName = false
      };

      //Act
      string statement = new CreateInsert().Generate(movie, insertProperties);

      //Assert
      statement.Should().Be(@$"INSERT INTO {nameof(Movie)} (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    }
  }
}