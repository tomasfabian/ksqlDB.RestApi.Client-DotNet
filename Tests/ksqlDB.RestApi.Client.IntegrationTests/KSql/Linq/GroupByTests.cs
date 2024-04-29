using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;

public class GroupByTests : Infrastructure.IntegrationTests
{
  [OneTimeSetUp]
  public static async Task ClassInitialize()
  {
    RestApiProvider = KSqlDbRestApiProvider.Create();
      
    var response = await RestApiProvider.CreateTypeAsync<TestState>();
    response = await RestApiProvider.CreateTypeAsync<TestCity>();

    response = await RestApiProvider.CreateTableAsync<TestCity>(new EntityCreationMetadata { EntityName = CitiesTableName, KafkaTopic = CitiesTableName, Partitions = 1});
    var content = await response.Content.ReadAsStringAsync();

    response = await RestApiProvider.InsertIntoAsync(new TestCity
      { RegionCode = "sk", State = State1 }, new InsertProperties { EntityName = CitiesTableName });
  }

  private static readonly TestState State1 = new() { Name = "Slovakia" };

  protected static string CitiesTableName => "test_cities";

  private IQbservable<TestCity> CitiesStream => Context.CreatePushQuery<TestCity>(CitiesTableName);

  record TestCity
  {
    [Key]
    public string RegionCode { get; init; } = null!;
    public TestState State { get; init; } = null!;
  }

  record TestState
  {
    public string Name { get; init; } = null!;
  }

  [Test]
  public async Task GroupByNested()
  {
    //Arrange
    int expectedItemsCount = 1;

    var source = CitiesStream
      .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
      .GroupBy(c => c.State.Name)
      .Select(g => new { g.Source.State.Name, Count = g.Count()})
      .Take(1)
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);

    actualValues[0].Count.Should().Be(1);
    actualValues[0].Name.Should().Be(State1.Name);
  }

  [Test]
  public async Task GroupByAnonymousNested()
  {
    //Arrange
    int expectedItemsCount = 1;

    var source = CitiesStream
      .GroupBy(c => new { c.RegionCode, c.State.Name })
      .Select(g => new { g.Source.RegionCode, g.Source.State.Name, Count = g.Count(c => c.RegionCode)})
      .Take(1)
      .ToAsyncEnumerable();

    //Act
    var actualValues = await CollectActualValues(source, expectedItemsCount);

    //Assert
    actualValues.Count.Should().Be(expectedItemsCount);

    actualValues[0].Count.Should().Be(1);
    actualValues[0].RegionCode.Should().Be("sk");
    actualValues[0].Name.Should().Be(State1.Name);
  }
}
