using System.Collections.Generic;
using FluentAssertions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Annotations;
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

    record EventCategory
    {
      public int Count { get; set; }
      public string Name { get; set; }
    }

    record Event
    {
      [Key]
      public int Id { get; set; }

      public string[] Places { get; set; }
    }

    [Test]
    public void StringEnumerableMemberType()
    {
      //Arrange
      var testEvent = new Event
      {
        Id = 1,
        Places = new[] { "Place1", "Place2", "Place3" },
      };

      //Act
      string statement = new CreateInsert().Generate(testEvent);

      //Assert
      statement.Should().Be(@"INSERT INTO Events (Id, Places) VALUES (1, ARRAY['Place1','Place2','Place3']);");
    }
    
    record EventWithPrimitiveEnumerable
    {
      [Key]
      public string Id { get; set; }

      public int[] Places { get; set; }
    }

    [Test]
    public void PrimitiveEnumerableMemberType()
    {
      //Arrange
      var testEvent = new EventWithPrimitiveEnumerable
      {
        Id = "1",
        Places = new[] { 1, 2, 3 }
      };

      //Act
      string statement = new CreateInsert().Generate(testEvent, new InsertProperties { EntityName = "Events"});

      //Assert
      statement.Should().Be(@"INSERT INTO Events (Id, Places) VALUES ('1', ARRAY[1,2,3]);");
    }
    
    record EventWithList
    {
      [Key]
      public string Id { get; set; }

      public List<int> Places { get; set; }
    }

    [Test]
    public void PrimitiveListMemberType()
    {
      //Arrange
      var testEvent = new EventWithList
      {
        Id = "1",
        Places = new List<int> { 1, 2, 3 }
      };

      //Act
      string statement = new CreateInsert().Generate(testEvent, new InsertProperties { EntityName = "Events"});

      //Assert
      statement.Should().Be(@"INSERT INTO Events (Id, Places) VALUES ('1', ARRAY[1,2,3]);");
    }

    [Test]
    public void ComplexListMemberType()
    {
    }

    struct ComplexEvent
    {
      [Key]
      public int Id { get; set; }

      public EventCategory Category { get; set; }
    }

    [Test]
    public void ComplexType()
    {
      //Arrange
      var eventCategory = new EventCategory
      {
        Count = 1,
        Name = "Planet Earth"
      };

      var testEvent = new ComplexEvent
      {
        Id = 1,
        Category = eventCategory
        //Categories = new[] { eventCategory, new EventCategory { Name = "Discovery" } }
      };

      //Act
      string statement = new CreateInsert().Generate(testEvent, new InsertProperties { EntityName = "Events"});

      //Assert
      statement.Should().Be(@"INSERT INTO Events (Id, Category) VALUES (1, STRUCT(Count := 1, Name := 'Planet Earth'));");
    }
  }
}