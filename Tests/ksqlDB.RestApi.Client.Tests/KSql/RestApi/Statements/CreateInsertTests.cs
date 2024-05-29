using System.Globalization;
using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDb.RestApi.Client.Tests.KSql.RestApi.Generators;
using ksqlDb.RestApi.Client.Tests.Models.Movies;
using NUnit.Framework;
using static ksqlDB.RestApi.Client.KSql.RestApi.Enums.IdentifierEscaping;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi.Statements;

public class CreateInsertTests
{
  private ModelBuilder modelBuilder = null!;

  [SetUp]
  public void Init()
  {
    modelBuilder = new();
  }

  public static IEnumerable<(IdentifierEscaping, string)> GenerateTestCases()
  {
    yield return (Never, "INSERT INTO Movies (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    yield return (Keywords, "INSERT INTO Movies (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    yield return (Always, "INSERT INTO `Movies` (`Title`, `Id`, `Release_Year`) VALUES ('Title', 1, 1988);");
  }

  [TestCaseSource(nameof(GenerateTestCases))]
  public void Generate((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title" };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(movie, new InsertProperties { IdentifierEscaping = escaping });

    //Assert
    statement.Should().Be(expected);
  }

  public static IEnumerable<(IdentifierEscaping, string)> GenerateOverrideEntityNameTestCases()
  {
    yield return (Never, "INSERT INTO TestNames (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    yield return (Keywords, "INSERT INTO TestNames (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
    yield return (Always, "INSERT INTO `TestNames` (`Title`, `Id`, `Release_Year`) VALUES ('Title', 1, 1988);");
  }

  [TestCaseSource(nameof(GenerateOverrideEntityNameTestCases))]
  public void Generate_OverrideEntityName((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title" };
    var insertProperties = new InsertProperties
    {
      EntityName = "TestName",
      IdentifierEscaping = escaping
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(movie, insertProperties);

    //Assert
    statement.Should().Be(expected);
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
    string statement = new CreateInsert(modelBuilder).Generate(movie, insertProperties);

    //Assert
    statement.Should().Be($"INSERT INTO {insertProperties.EntityName} (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
  }

  [Test]
  public void Generate_UseModelBuilder_Ignore()
  {
    //Arrange
    modelBuilder.Entity<Movie>().Property(c => c.Release_Year).Ignore();

    var movie = new Movie { Id = 1, Release_Year = 1988, Title = "Title" };
    var insertProperties = new InsertProperties
    {
      EntityName = "TestName",
      ShouldPluralizeEntityName = false
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(movie, insertProperties);

    //Assert
    statement.Should().Be($"INSERT INTO {insertProperties.EntityName} (Title, Id) VALUES ('Title', 1);");
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
    string statement = new CreateInsert(modelBuilder).Generate(movie, insertProperties);

    //Assert
    statement.Should().Be($"INSERT INTO {nameof(Movie)} (Title, Id, Release_Year) VALUES ('Title', 1, 1988);");
  }

  public record Book(string Title, string Author);

  [Test]
  public void Generate_ImmutableRecordType()
  {
    //Arrange
    var book = new Book("Title", "Author");

    var insertProperties = new InsertProperties
    {
      ShouldPluralizeEntityName = false
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(book, insertProperties);

    //Assert
    statement.Should().Be($"INSERT INTO {nameof(Book)} (Title, Author) VALUES ('Title', 'Author');");
  }

  class Pen(string colour)
  {
    [JsonPropertyName("color")] public string Colour { get; set; } = colour;
  }

  [TestCase(Never, ExpectedResult = "INSERT INTO Pen (color) VALUES ('red');")]
  [TestCase(Keywords, ExpectedResult = "INSERT INTO Pen (color) VALUES ('red');")]
  [TestCase(Always, ExpectedResult = "INSERT INTO `Pen` (`color`) VALUES ('red');")]
  public string Generate_JsonPropertyName(IdentifierEscaping escaping)
  {
    //Arrange
    var pen = new Pen("red");

    var insertProperties = new InsertProperties
    {
      ShouldPluralizeEntityName = false,
      IdentifierEscaping = escaping
    };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(pen, insertProperties);

    //Assert
    return statement;
  }

  public class Star(string name)
  {
    [JsonPropertyName("name")] public string Name { get; set; } = name;
  }

  [Struct]
  class BinaryStar(Star star1, Star star2)
  {
    [JsonPropertyName("first_star")] public Star Star1 { get; set; } = star1;
    [JsonPropertyName("second_star")] public Star Star2 { get; set; } = star2;
  }

  [TestCase(Never,
    ExpectedResult =
      "INSERT INTO BinaryStar (first_star, second_star) VALUES (STRUCT(name := 'Alpha Centauri A'), STRUCT(name := 'Alpha Centauri B'));")]
  [TestCase(Keywords,
    ExpectedResult =
      "INSERT INTO BinaryStar (first_star, second_star) VALUES (STRUCT(name := 'Alpha Centauri A'), STRUCT(name := 'Alpha Centauri B'));")]
  [TestCase(Always,
    ExpectedResult =
      "INSERT INTO `BinaryStar` (`first_star`, `second_star`) VALUES (STRUCT(`name` := 'Alpha Centauri A'), STRUCT(`name` := 'Alpha Centauri B'));")]
  public string Generate_JsonPropertyNameInStruct(IdentifierEscaping escaping)
  {
    //Arrange
    var binaryStar = new BinaryStar(new Star("Alpha Centauri A"), new Star("Alpha Centauri B"));

    var insertProperties = new InsertProperties
    {
      ShouldPluralizeEntityName = false,
      IdentifierEscaping = escaping
    };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(binaryStar, insertProperties);

    //Assert
    return statement;
  }

  record EventCategory
  {
    public int Count { get; set; }
    public string Name { get; init; } = null!;
  }

  record Event
  {
    [Key]
    public int Id { get; set; }

    public string[] Places { get; set; } = null!;
  }

  [Test]
  public void StringEnumerableMemberType()
  {
    //Arrange
    var testEvent = new Event
    {
      Id = 1,
      Places = ["Place1", "Place2", "Place3"],
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(testEvent);

    //Assert
    statement.Should().Be("INSERT INTO Events (Id, Places) VALUES (1, ARRAY['Place1', 'Place2', 'Place3']);");
  }

  record EventWithPrimitiveEnumerable
  {
    [Key]
    public string Id { get; init; } = null!;

    public int[] Places { get; init; } = null!;
  }

  [Test]
  public void PrimitiveEnumerableMemberType()
  {
    //Arrange
    var testEvent = new EventWithPrimitiveEnumerable
    {
      Id = "1",
      Places = [1, 2, 3]
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(testEvent, new InsertProperties { EntityName = "Events" });

    //Assert
    statement.Should().Be("INSERT INTO Events (Id, Places) VALUES ('1', ARRAY[1, 2, 3]);");
  }

  record EventWithList
  {
    [Key]
    public string Id { get; set; } = null!;

    public List<int> Places { get; set; } = null!;
  }

  [Test]
  public void PrimitiveListMemberType()
  {
    //Arrange
    var testEvent = new EventWithList
    {
      Id = "1",
      Places = [1, 2, 3]
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(testEvent, new InsertProperties { EntityName = "Events" });

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

  public static IEnumerable<(IdentifierEscaping, string)> ComplexTypeTestCases()
  {
    yield return (Never, "INSERT INTO Events (Id, Category) VALUES (1, STRUCT(Count := 1, Name := 'Planet Earth'));");
    yield return (Keywords, "INSERT INTO Events (Id, Category) VALUES (1, STRUCT(Count := 1, Name := 'Planet Earth'));");
    yield return (Always,
      "INSERT INTO `Events` (`Id`, `Category`) VALUES (1, STRUCT(`Count` := 1, `Name` := 'Planet Earth'));");
  }

  [TestCaseSource(nameof(ComplexTypeTestCases))]
  public void ComplexType((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
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

    var insertProperties = new InsertProperties { EntityName = "Events", IdentifierEscaping = escaping };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(testEvent, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  public static IEnumerable<(IdentifierEscaping, string)> ComplexTypeNullReferenceTestCases()
  {
    yield return (Never, "INSERT INTO Events (Id, Category) VALUES (1, NULL);");
    yield return (Keywords, "INSERT INTO Events (Id, Category) VALUES (1, NULL);");
    yield return (Always, "INSERT INTO `Events` (`Id`, `Category`) VALUES (1, NULL);");
  }

  [TestCaseSource(nameof(ComplexTypeNullReferenceTestCases))]
  public void ComplexType_NullReferenceValue((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var testEvent = new ComplexEvent
    {
      Id = 1,
      Category = null!
    };

    var insertProperties = new InsertProperties { EntityName = "Events", IdentifierEscaping = escaping };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(testEvent, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  public class Kafka_table_order
  {
    public int Id { get; set; }
    public IEnumerable<double> Items { get; set; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> IncludeReadOnlyPropertiedTestCases()
  {
    yield return (Never, $"INSERT INTO {nameof(Movie)}s (Title, Id, Release_Year, ReadOnly) VALUES (NULL, 1, 0, ARRAY[1, 2]);");
    yield return (Keywords, $"INSERT INTO {nameof(Movie)}s (Title, Id, Release_Year, ReadOnly) VALUES (NULL, 1, 0, ARRAY[1, 2]);");
    yield return (Always, $"INSERT INTO `{nameof(Movie)}s` (`Title`, `Id`, `Release_Year`, `ReadOnly`) VALUES (NULL, 1, 0, ARRAY[1, 2]);");
  }

  [TestCaseSource(nameof(IncludeReadOnlyPropertiedTestCases))]
  public void IncludeReadOnlyProperties((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var order = new Movie { Id = 1 };

    var insertProperties = new InsertProperties
    {
      IncludeReadOnlyProperties = true,
      IdentifierEscaping = escaping
    };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(order, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  public static IEnumerable<(IdentifierEscaping, string)> EnumerableTestCases()
  {
    yield return (Never, "INSERT INTO Kafka_table_orders (Id, Items) VALUES (1, ARRAY[1, 2, 3]);");
    yield return (Keywords, "INSERT INTO Kafka_table_orders (Id, Items) VALUES (1, ARRAY[1, 2, 3]);");
    yield return (Always, "INSERT INTO `Kafka_table_orders` (`Id`, `Items`) VALUES (1, ARRAY[1, 2, 3]);");
  }

  [TestCaseSource(nameof(EnumerableTestCases))]
  public void Enumerable((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var order = new Kafka_table_order { Id = 1, Items = System.Linq.Enumerable.Range(1, 3).Select(c => (double)c) };
    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(order, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  [Test]
  public void FromList()
  {
    //Arrange
    var order = new Kafka_table_order { Id = 1, Items = [1.1, 2] };

    var insertProperties = new InsertProperties
    {
      FormatDoubleValue = value => value.ToString(CultureInfo.InvariantCulture)
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(order, insertProperties);

    //Assert
    statement.Should().Be($"INSERT INTO Kafka_table_orders ({nameof(Kafka_table_order.Id)}, {nameof(Kafka_table_order.Items)}) VALUES (1, ARRAY[1.1, 2]);");
  }

  record Kafka_table_order2
  {
    public int Id { get; set; }
    public List<double>? ItemsList { get; set; }
  }

  [Test]
  public void List()
  {
    //Arrange
    var order = new Kafka_table_order2 { Id = 1, ItemsList = [1.1, 2] };

    var config = new InsertProperties
    {
      ShouldPluralizeEntityName = false,
      FormatDoubleValue = value => value.ToString(CultureInfo.InvariantCulture),
      EntityName = "`my_order`"
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(order, config);

    //Assert
    statement.Should().Be("INSERT INTO `my_order` (Id, ItemsList) VALUES (1, ARRAY[1.1, 2]);");
  }

  [Test]
  public void FromEmptyList()
  {
    //Arrange
    var order = new Kafka_table_order2 { Id = 1, ItemsList = [] };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(order);

    //Assert
    statement.Should().Be("INSERT INTO Kafka_table_order2s (Id, ItemsList) VALUES (1, ARRAY_REMOVE(ARRAY[0], 0));"); //ARRAY[] is not supported
  }

  [Test]
  public void FromNullList()
  {
    //Arrange
    var order = new Kafka_table_order2 { Id = 1, ItemsList = null };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(order);

    //Assert
    statement.Should().Be("INSERT INTO Kafka_table_order2s (Id, ItemsList) VALUES (1, NULL);");
  }

  record Kafka_table_order3
  {
    public int Id { get; set; }
    public IList<int> ItemsList { get; set; } = null!;
  }

  [Test]
  public void ListInterface()
  {
    //Arrange
    var order = new Kafka_table_order3 { Id = 1, ItemsList = [1, 2] };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(order, new InsertProperties { ShouldPluralizeEntityName = false, EntityName = nameof(Kafka_table_order) });

    //Assert
    statement.Should().Be("INSERT INTO Kafka_table_order (Id, ItemsList) VALUES (1, ARRAY[1, 2]);");
  }

  record FooNestedArrayInMap
  {
    public Dictionary<string, int[]> Map { get; set; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> NestedArrayInMapTestCases()
  {
    yield return (Never, "INSERT INTO FooNestedArrayInMaps (Map) VALUES (MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4]));");
    yield return (Keywords, "INSERT INTO FooNestedArrayInMaps (Map) VALUES (MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4]));");
    yield return (Always, "INSERT INTO `FooNestedArrayInMaps` (`Map`) VALUES (MAP('a' := ARRAY[1, 2], 'b' := ARRAY[3, 4]));");
  }

  [TestCaseSource(nameof(NestedArrayInMapTestCases))]
  public void NestedArrayInMap((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var order = new FooNestedArrayInMap
    {
      Map = new Dictionary<string, int[]>
      {
        { "a", [1, 2]},
        { "b", [3, 4]},
      }
    };

    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };
    //Act
    var statement = new CreateInsert(modelBuilder).Generate(order, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  record FooNestedMapInMap
  {
    public Dictionary<string, Dictionary<string, int>> Map { get; set; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> NestedMapInMapTestCases()
  {
    yield return (Never,
      "INSERT INTO FooNestedMapInMaps (Map) VALUES (MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4)));");
    yield return (Keywords,
      "INSERT INTO FooNestedMapInMaps (Map) VALUES (MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4)));");
    yield return (Always,
      "INSERT INTO `FooNestedMapInMaps` (`Map`) VALUES (MAP('a' := MAP('a' := 1, 'b' := 2), 'b' := MAP('c' := 3, 'd' := 4)));");
  }

  [TestCaseSource(nameof(NestedMapInMapTestCases))]
  public void NestedMapInMap((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var value = new FooNestedMapInMap
    {
      Map = new Dictionary<string, Dictionary<string, int>>
      {
        { "a", new Dictionary<string, int> { { "a", 1 }, { "b", 2 } } },
        { "b", new Dictionary<string, int> { { "c", 3 }, { "d", 4 } } },
      }
    };

    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    statement.Should().Be(expected);
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
    public string Property { get; init; } = null!;
    public FooNestedStruct Str { get; init; } = null!;
  }

  record FooNestedStruct
  {
    public int NestedProperty { get; set; }
    public LocationStruct Nested { get; set; }
  }

  public static IEnumerable<(IdentifierEscaping, string)> NestedStructTestCases()
  {
    yield return (Never,
      "INSERT INTO FooStructs (Property, Str) VALUES ('ET', STRUCT(NestedProperty := 2, Nested := STRUCT(X := 'test', Y := 1)));");
    yield return (Keywords,
      "INSERT INTO FooStructs (Property, Str) VALUES ('ET', STRUCT(NestedProperty := 2, Nested := STRUCT(X := 'test', Y := 1)));");
    yield return (Always,
      "INSERT INTO `FooStructs` (`Property`, `Str`) VALUES ('ET', STRUCT(`NestedProperty` := 2, `Nested` := STRUCT(`X` := 'test', `Y` := 1)));");
  }

  [TestCaseSource(nameof(NestedStructTestCases))]
  public void NestedStruct((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
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

    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  record FooNestedStructInMap
  {
    public Dictionary<string, LocationStruct> Str { get; set; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> NestedStructInMapTestCases()
  {
    yield return (Never,
      "INSERT INTO FooNestedStructInMaps (Str) VALUES (MAP('a' := STRUCT(X := 'go', Y := 2), 'b' := STRUCT(X := 'test', Y := 1)));");
    yield return (Keywords,
      "INSERT INTO FooNestedStructInMaps (Str) VALUES (MAP('a' := STRUCT(X := 'go', Y := 2), 'b' := STRUCT(X := 'test', Y := 1)));");
    yield return (Always,
      "INSERT INTO `FooNestedStructInMaps` (`Str`) VALUES (MAP('a' := STRUCT(`X` := 'go', `Y` := 2), 'b' := STRUCT(`X` := 'test', `Y` := 1)));");
  }

  [TestCaseSource(nameof(NestedStructInMapTestCases))]
  public void NestedStructInMap((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
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

    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };
    //Act
    var statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  record FooNestedMapInArray
  {
    public Dictionary<string, int>[] Arr { get; init; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> NestedMapInArrayTestCases()
  {
    yield return (Never,
      "INSERT INTO FooNestedMapInArrays (Arr) VALUES (ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)]);");
    yield return (Keywords,
      "INSERT INTO FooNestedMapInArrays (Arr) VALUES (ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)]);");
    yield return (Always,
      "INSERT INTO `FooNestedMapInArrays` (`Arr`) VALUES (ARRAY[MAP('a' := 1, 'b' := 2), MAP('c' := 3, 'd' := 4)]);");
  }

  [TestCaseSource(nameof(NestedMapInArrayTestCases))]
  public void NestedMapInArray((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var value = new FooNestedMapInArray
    {
      Arr =
      [
        new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
        new Dictionary<string, int> { { "c", 3 }, { "d", 4 } }
      ]
    };

    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  record FooNestedArrayInArray
  {
    public int[][] Arr { get; init; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> NestedArrayInArrayTestCases()
  {
    yield return (Never, "INSERT INTO FooNestedArrayInArrays (Arr) VALUES (ARRAY[ARRAY[1, 2], ARRAY[3, 4]]);");
    yield return (Keywords, "INSERT INTO FooNestedArrayInArrays (Arr) VALUES (ARRAY[ARRAY[1, 2], ARRAY[3, 4]]);");
    yield return (Always, "INSERT INTO `FooNestedArrayInArrays` (`Arr`) VALUES (ARRAY[ARRAY[1, 2], ARRAY[3, 4]]);");
  }

  [TestCaseSource(nameof(NestedArrayInArrayTestCases))]
  public void NestedArrayInArray((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var value = new FooNestedArrayInArray
    {
      Arr =
      [
        [1, 2],
        [3, 4]
      ]
    };

    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  record FooNestedStructInArray
  {
    public LocationStruct[] Arr { get; set; } = null!;
  }

  public static IEnumerable<(IdentifierEscaping, string)> NestedStructInArrayTestCases()
  {
    yield return (Never,
      "INSERT INTO FooNestedStructInArrays (Arr) VALUES (ARRAY[STRUCT(X := 'go', Y := 2), STRUCT(X := 'test', Y := 1)]);");
    yield return (Keywords,
      "INSERT INTO FooNestedStructInArrays (Arr) VALUES (ARRAY[STRUCT(X := 'go', Y := 2), STRUCT(X := 'test', Y := 1)]);");
    yield return (Always,
      "INSERT INTO `FooNestedStructInArrays` (`Arr`) VALUES (ARRAY[STRUCT(`X` := 'go', `Y` := 2), STRUCT(`X` := 'test', `Y` := 1)]);");
  }

  [TestCaseSource(nameof(NestedStructInArrayTestCases))]
  public void NestedStructInArray((IdentifierEscaping escaping, string expected) testCase)
  {
    //Arrange
    var (escaping, expected) = testCase;
    var value = new FooNestedStructInArray
    {
      Arr =
      [
        new LocationStruct
        {
          X = "go",
          Y = 2,
        }, new LocationStruct
        {
          X = "test",
          Y = 1,
        }
      ]
    };

    var insertProperties = new InsertProperties { IdentifierEscaping = escaping };

    //Act
    var statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    statement.Should().Be(expected);
  }

  [Test]
  public void TimeTypes_DateAndTime()
  {
    //Arrange
    var value = new CreateEntityTests.TimeTypes()
    {
      Dt = new DateTime(2021, 2, 3),
      Ts = new TimeSpan(2, 3, 4)
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value);

    //Assert
    statement.Should().Be("INSERT INTO TimeTypes (Dt, Ts, DtOffset) VALUES ('2021-02-03', '02:03:04', '0001-01-01T00:00:00.000+00:00');");
  }

  private record TimeTypes
  {
    public DateTimeOffset DtOffset { get; set; }
  }

  [Test]
  public void TimeTypes_Timespan()
  {
    //Arrange
    var value = new TimeTypes()
    {
      DtOffset = new DateTimeOffset(2021, 7, 4, 13, 29, 45, 447, TimeSpan.FromHours(4))
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value);

    //Assert
    statement.Should().Be("INSERT INTO TimeTypes (DtOffset) VALUES ('2021-07-04T13:29:45.447+04:00');");
  }

  internal record GuidKey
  {
    public Guid DataId { get; set; }
  }

  [Test]
  public void GuidType()
  {
    //Arrange
    var value = new GuidKey
    {
      DataId = Guid.NewGuid()
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value, new InsertProperties { EntityName = "GuidKeys" });

    //Assert
    statement.Should().Be($"INSERT INTO GuidKeys (DataId) VALUES ('{value.DataId}');");
  }

  private record Update
  {
    public string ExtraField = "Test value";
  }

  [Test]
  public void Field()
  {
    //Arrange
    var value = new Update();

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value);

    //Assert
    statement.Should().Be($"INSERT INTO Updates ({nameof(Update.ExtraField)}) VALUES ('{value.ExtraField}');");
  }

  private interface IMyUpdate
  {
    public string Field { get; set; }
  }

  private record MyUpdate : IMyUpdate
  {
    public string ExtraField = "Test value";
    public string Field { get; set; } = null!;
    public string Field2 { get; init; } = null!;
  }

  [Test]
  public void Generate_FromInterface_UseEntityType()
  {
    //Arrange
    IMyUpdate value = new MyUpdate
    {
      Field = "Value",
      Field2 = "Value2",
    };

    var insertProperties = new InsertProperties
    {
      EntityName = nameof(MyUpdate),
      ShouldPluralizeEntityName = false,
      UseInstanceType = true
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    var myUpdate = (MyUpdate)value;
    statement.Should().Be($"INSERT INTO {nameof(MyUpdate)} ({nameof(IMyUpdate.Field)}, {nameof(MyUpdate.Field2)}, {nameof(MyUpdate.ExtraField)}) VALUES ('{value.Field}', '{myUpdate.Field2}', '{myUpdate.ExtraField}');");
  }

  [Test]
  public void Generate_FromInterface_DoNotUseEntityType()
  {
    //Arrange
    IMyUpdate value = new MyUpdate
    {
      Field = "Value",
      Field2 = "Value2",
    };

    var insertProperties = new InsertProperties
    {
      EntityName = nameof(MyUpdate),
      ShouldPluralizeEntityName = false
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value, insertProperties);

    //Assert
    statement.Should().Be($"INSERT INTO {nameof(MyUpdate)} ({nameof(IMyUpdate.Field)}) VALUES ('{value.Field}');");
  }

  [Test]
  public void Generate_FromInterface_WithNullInsertProperties_DoNotUseEntityType()
  {
    //Arrange
    IMyUpdate value = new MyUpdate
    {
      Field = "Value",
      Field2 = "Value2",
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value, insertProperties: null);

    //Assert
    statement.Should().Be($"INSERT INTO {nameof(IMyUpdate)}s ({nameof(IMyUpdate.Field)}) VALUES ('{value.Field}');");
  }

  [Test]
  public void Generate_Enum()
  {
    //Arrange
    var value = new Port
    {
      Id = 42,
      PortType = PortType.Snowflake,
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(value, insertProperties: null);

    //Assert
    statement.Should().Be($"INSERT INTO {nameof(Port)}s ({nameof(Port.Id)}, {nameof(Port.PortType)}) VALUES (42, '{value.PortType}');");
  }

  private record Amount
  {
    [JsonPropertyName("volume")]
    [Decimal(20, 8)]
    public decimal Volume { get; init; }
  }

  [Test]
  public void Generate_Decimal_ShouldBePrinted()
  {
    //Arrange
    var amount = new Amount
    {
      Volume = 1.12345678912345M,
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(amount);

    //Assert
    statement.Should().Be($"INSERT INTO {nameof(Amount)}s ({nameof(Amount.Volume).ToLower()}) VALUES ({amount.Volume});");
  }

  #region TODO insert with functions

  readonly struct MovieBytes
  {
    public string Title { get; init; }
    public byte[] RawTitle { get; init; }
  }

  [Test]
  [Ignore("TODO")]
  public void Bytes()
  {
    //Arrange
    var movie = new MovieBytes
    {
      Title = "Alien",
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(movie, new InsertProperties { EntityName = "Movies" });

    //Assert
    statement.Should().Be("INSERT INTO Movies (Title) VALUES (TO_BYTES('Alien', 'utf8'));");
  }

  readonly struct Invoke
  {
    public string Title { get; init; }

    public static Func<string, string> TitleConverter => c => K.Functions.Concat(c, "_new");
  }

  [Test]
  [Ignore("TODO")]
  public void Concat()
  {
    //Arrange
    var movie = new Invoke
    {
      Title = "Alien"
    };

    //Act
    string statement = new CreateInsert(modelBuilder).Generate(movie, new InsertProperties { EntityName = "Movies" });

    //Assert
    statement.Should().Be("INSERT INTO Movies (Title) VALUES (CONCAT('Alien', '_new'));");
  }

  #endregion
}
