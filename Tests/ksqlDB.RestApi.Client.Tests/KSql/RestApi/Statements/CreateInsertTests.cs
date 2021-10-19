using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Models.Movies;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi.Statements
{
  public class CreateInsertTests
  {
    [Test]
    public void Generate()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title" };

      //Act
      string statement = new CreateInsert().Generate(movie, null);

      //Assert
      statement.Should().Be(@"INSERT INTO Movies (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    }

    [Test]
    public void Generate_OverrideEntityName()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title" };
      var insertProperties = new InsertProperties
      {
        EntityName = "TestName"
      };

      //Act
      string statement = new CreateInsert().Generate(movie, insertProperties);

      //Assert
      statement.Should().Be($"INSERT INTO {insertProperties.EntityName}s (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    }

    [Test]
    public void Generate_OverrideEntityName_ShouldNotPluralize()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title" };
      var insertProperties = new InsertProperties
      {
        EntityName = "TestName",
        ShouldPluralizeEntityName = false
      };

      //Act
      string statement = new CreateInsert().Generate(movie, insertProperties);

      //Assert
      statement.Should().Be($"INSERT INTO {insertProperties.EntityName} (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    }

    [Test]
    public void Generate_ShouldNotPluralizeEntityName()
    {
      //Arrange
      var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title" };
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
      statement.Should().Be("INSERT INTO Events (Id, Places) VALUES (1, ARRAY['Place1', 'Place2', 'Place3']);");
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
      string statement = new CreateInsert().Generate(testEvent, new InsertProperties { EntityName = "Events" });

      //Assert
      statement.Should().Be("INSERT INTO Events (Id, Places) VALUES ('1', ARRAY[1, 2, 3]);");
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
      string statement = new CreateInsert().Generate(testEvent, new InsertProperties { EntityName = "Events" });

      //Assert
      statement.Should().Be("INSERT INTO Events (Id, Places) VALUES ('1', ARRAY[1, 2, 3]);");
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
      string statement = new CreateInsert().Generate(testEvent, new InsertProperties { EntityName = "Events" });

      //Assert
      statement.Should().Be("INSERT INTO Events (Id, Category) VALUES (1, STRUCT(Count := 1, Name := 'Planet Earth'));");
    }

    [Test]
    public void ComplexType_NullReferenceValue()
    {
      //Arrange
      var testEvent = new ComplexEvent
      {
        Id = 1,
        Category = null
      };

      //Act
      string statement = new CreateInsert().Generate(testEvent, new InsertProperties { EntityName = "Events" });

      //Assert
      statement.Should().Be("INSERT INTO Events (Id, Category) VALUES (1, NULL);");
    }

    record Kafka_table_order
    {
      public int Id { get; set; }
      public IEnumerable<double> Items { get; set; }
    }

    [Test]
    public void Enumerable()
    {
      //Arrange
      var order = new Kafka_table_order { Id = 1, Items = System.Linq.Enumerable.Range(1, 3).Select(c => (double)c) };

      //Act
      string statement = new CreateInsert().Generate(order, null);

      //Assert
      statement.Should().Be("INSERT INTO Kafka_table_orders (Id, Items) VALUES (1, ARRAY[1, 2, 3]);");
    }

    [Test]
    public void FromList()
    {
      //Arrange
      var order = new Kafka_table_order { Id = 1, Items = new List<double> { 1.1, 2 } };

      var insertProperties = new InsertProperties
      {
        FormatDoubleValue = value => value.ToString(CultureInfo.InvariantCulture)
      };
      //Act
      string statement = new CreateInsert().Generate(order, insertProperties);

      //Assert
      statement.Should().Be($"INSERT INTO Kafka_table_orders ({nameof(Kafka_table_order.Id)}, {nameof(Kafka_table_order.Items)}) VALUES (1, ARRAY[1.1, 2]);");
    }


    record Kafka_table_order2
    {
      public int Id { get; set; }
      public List<double> ItemsList { get; set; }
    }

    [Test]
    public void List()
    {
      //Arrange
      var order = new Kafka_table_order2 { Id = 1, ItemsList = new List<double> { 1.1, 2 } };

      var config = new InsertProperties
      {
        ShouldPluralizeEntityName = false,
        FormatDoubleValue = value => value.ToString(CultureInfo.InvariantCulture),
        EntityName = "`my_order`"
      };

      //Act
      string statement = new CreateInsert().Generate(order, config);

      //Assert
      statement.Should().Be("INSERT INTO `my_order` (Id, ItemsList) VALUES (1, ARRAY[1.1, 2]);");
    }

    [Test]
    public void FromEmptyList()
    {
      //Arrange
      var order = new Kafka_table_order2 { Id = 1, ItemsList = new List<double>() };

      //Act
      string statement = new CreateInsert().Generate(order);

      //Assert
      statement.Should().Be("INSERT INTO Kafka_table_order2s (Id, ItemsList) VALUES (1, ARRAY_REMOVE(ARRAY[0], 0));"); //ARRAY[] is not supported
    }

    [Test]
    public void FromNullList()
    {
      //Arrange
      var order = new Kafka_table_order2 { Id = 1, ItemsList = null };

      //Act
      string statement = new CreateInsert().Generate(order);

      //Assert
      statement.Should().Be("INSERT INTO Kafka_table_order2s (Id, ItemsList) VALUES (1, NULL);");
    }

    record Kafka_table_order3
    {
      public int Id { get; set; }
      public IList<int> ItemsList { get; set; }
    }

    [Test]
    public void ListInterface()
    {
      //Arrange
      var order = new Kafka_table_order3 { Id = 1, ItemsList = new List<int> { 1, 2 } };

      //Act
      string statement = new CreateInsert().Generate(order, new InsertProperties { ShouldPluralizeEntityName = false, EntityName = nameof(Kafka_table_order) });

      //Assert
      statement.Should().Be("INSERT INTO Kafka_table_order (Id, ItemsList) VALUES (1, ARRAY[1, 2]);");
    }

    record FooNestedArrayInMap
    {
      public Dictionary<string, int[]> Map { get; set; }
    }

    [Test]
    public void NestedArrayInMap()
    {
      //Arrange
      var order = new FooNestedArrayInMap
      {
        Map = new Dictionary<string, int[]>
                          {
                            { "a", new[] { 1, 2 } },
                            { "b", new[] { 3, 4 } },
                          }

      };

      //Act
      string statement = new CreateInsert().Generate(order);

      //Assert
      statement.Should().Be("INSERT INTO FooNestedArrayInMaps (Map) VALUES (MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4]));");
    }

    record FooNestedMapInMap
    {
      public Dictionary<string, Dictionary<string, int>> Map { get; set; }
    }

    [Test]
    public void NestedMapInMap()
    {
      //Arrange
      var value = new FooNestedMapInMap
      {
        Map = new Dictionary<string, Dictionary<string, int>>
              {
                { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
                { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
              }

      };

      //Act
      string statement = new CreateInsert().Generate(value);

      //Assert
      statement.Should().Be("INSERT INTO FooNestedMapInMaps (Map) VALUES (MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4)));");
    }

    private struct LocationStruct
    {
      public string X { get; set; }
      public double Y { get; set; }
      //public string[] Arr { get; set; }
      //public Dictionary<string, double> Map { get; set; }
    }

    record FooStruct
    {
      public string Property { get; set; }
      public FooNestedStruct Str { get; set; }
    }

    record FooNestedStruct
    {
      public int NestedProperty { get; set; }
      public LocationStruct Nested { get; set; }
    }

    [Test]
    public void NestedStruct()
    {
      //Arrange
      var value = new FooStruct
                  {
                    Property = "ET",
                    Str = new FooNestedStruct
                    {
                            NestedProperty = 2,
                            Nested = new LocationStruct
                            {
                              X = "test",
                              Y = 1,
                            }
                          }
      };

      //Act
      string statement = new CreateInsert().Generate(value);

      //Assert
      statement.Should().Be("INSERT INTO FooStructs (Property, Str) VALUES ('ET', STRUCT(NestedProperty := 2, Nested := STRUCT(X := 'test', Y := 1)));");
    }

    record FooNestedStructInMap
    {
      public Dictionary<string, LocationStruct> Str { get; set; }
    }

    [Test]
    public void NestedStructInMap()
    {
      //Arrange
      var value = new FooNestedStructInMap
      {
        Str = new Dictionary<string, LocationStruct>
                          {
                            { "a", new LocationStruct
                                   {
                                     X = "go",
                                     Y = 2,
                                   } },
                            { "b", new LocationStruct
                                   {
                                     X = "test",
                                     Y = 1,
                                   }
                            }
                          }
      };

      //Act
      string statement = new CreateInsert().Generate(value);

      //Assert
      statement.Should().Be("INSERT INTO FooNestedStructInMaps (Str) VALUES (MAP('a' := STRUCT(X := 'go', Y := 2), 'b' := STRUCT(X := 'test', Y := 1)));");
    }

    record FooNestedMapInArray
    {
      public Dictionary<string, int>[] Arr { get; set; }
    }

    [Test]
    public void NestedMapInArray()
    {
      //Arrange
      var value = new FooNestedMapInArray
      {
        Arr = new[]
              {
                new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
                new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
              }
      };

      //Act
      string statement = new CreateInsert().Generate(value);

      //Assert
      statement.Should().Be("INSERT INTO FooNestedMapInArrays (Arr) VALUES (ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)]);");
    }

    record FooNestedArrayInArray
    {
      public int[][] Arr { get; set; }
    }

    [Test]
    public void NestedArrayInArray()
    {
      //Arrange
      var value = new FooNestedArrayInArray
      {
        Arr = new[]
              {
                new [] { 1, 2},
                new [] { 3, 4},
              }
      };

      //Act
      string statement = new CreateInsert().Generate(value);

      //Assert
      statement.Should().Be("INSERT INTO FooNestedArrayInArrays (Arr) VALUES (ARRAY[ARRAY[1, 2], ARRAY[3, 4]]);");
    }

    record FooNestedStructInArray
    {
      public LocationStruct[] Arr { get; set; }
    }

    [Test]
    public void NestedStructInArray()
    {
      //Arrange
      var value = new FooNestedStructInArray
      {
        Arr = new[]
              {
                new LocationStruct
                {
                  X = "go",
                  Y = 2,
                }, new LocationStruct
                  {
                    X = "test",
                    Y = 1,
                  }
                }
      };

      //Act
      string statement = new CreateInsert().Generate(value);

      //Assert
      statement.Should().Be("INSERT INTO FooNestedStructInArrays (Arr) VALUES (ARRAY[STRUCT(X := 'go', Y := 2), STRUCT(X := 'test', Y := 1)]);");
    }

    #region TODO insert with functions

    struct MovieBytes
    {
      public string Title { get; init; }
      public byte[] RawTitle { get; init; }
    }

    [Test]
    [NUnit.Framework.Ignore("TODO")]
    public void Bytes()
    {
      //Arrange
      var movie = new MovieBytes
      {
        Title = "Alien",
      };

      //Act
      string statement = new CreateInsert().Generate(movie, new InsertProperties { EntityName = "Movies" });

      //Assert
      statement.Should().Be(@"INSERT INTO Movies (Title) VALUES (TO_BYTES('Alien', 'utf8'));");
    }

    struct Invoke
    {
      public string Title { get; init; }

      public Func<string, string> TitleConverter => c => K.Functions.Concat(c, "_new");
      //public Expression<Func<string, string>> TitleConverter => c => K.Functions.Concat(c, "_new");
      //public Expression<Func<Invoke, string>> TitleConverter => c => K.Functions.Concat(c.Title, "_new");
    }

    [Test]
    [NUnit.Framework.Ignore("TODO")]
    public void Concat()
    {
      //Arrange
      var movie = new Invoke
      {
        Title = "Alien"
      };

      //Act
      string statement = new CreateInsert().Generate(movie, new InsertProperties { EntityName = "Movies" });

      //Assert
      statement.Should().Be(@"INSERT INTO Movies (Title) VALUES (CONCAT('Alien', '_new'));");
    }

    #endregion
  }
}