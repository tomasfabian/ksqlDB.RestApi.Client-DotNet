using System.Text.Json.Serialization;
using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.IntegrationTests.Helpers;
using ksqlDb.RestApi.Client.IntegrationTests.Models;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq
{
  public class ModelBuilderTests : Infrastructure.IntegrationTests
  {
    protected static string StreamName = nameof(ModelBuilderTests);
    private static readonly string TopicName = StreamName;
    private static KSqlDbRestApiClient kSqlDbRestApiClient = null!;
    private static ModelBuilder modelBuilder = null!;

    [OneTimeSetUp]
    public static async Task ClassInitialize()
    {
      await InitializeDatabase();
    }

    public record Tweet : Record
    {
      public int Id { get; set; }
      [JsonPropertyName("MESSAGE")]
      public string Message { get; set; } = null!;
      public bool IsRobot { get; set; }
      public double Amount { get; set; }
      public decimal AccountBalance { get; set; }
    }

    public static readonly Tweet Tweet1 = new()
    {
      Id = 1,
      Message = "Hello world",
      IsRobot = true,
      Amount = 0.00042,
      RowTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };

    public static readonly Tweet Tweet2 = new()
    {
      Id = 2,
      Message = "Wall-e",
      IsRobot = false,
      Amount = 1,
      RowTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    };

    protected static async Task InitializeDatabase()
    {
      modelBuilder = new ModelBuilder();
      modelBuilder.Entity<Tweet>()
        .Property(c => c.Id)
        .HasColumnName("TweetId");
      modelBuilder.Entity<Tweet>()
        .Property(c => c.AccountBalance)
        .Ignore();

      var httpClient = new HttpClient
      {
        BaseAddress = new Uri(TestConfig.KSqlDbUrl)
      };
      kSqlDbRestApiClient = new KSqlDbRestApiClient(new HttpClientFactory(httpClient), modelBuilder);

      await ClassCleanup();

      var entityCreationMetadata = new EntityCreationMetadata(TopicName, 1)
      {
        EntityName = StreamName,
        ShouldPluralizeEntityName = false,
        IdentifierEscaping = IdentifierEscaping.Always
      };
      var result = await kSqlDbRestApiClient.CreateStreamAsync<Tweet>(entityCreationMetadata, true);
      result.IsSuccess().Should().BeTrue();

      var insertProperties = new InsertProperties()
      {
        EntityName = StreamName,
        IdentifierEscaping = IdentifierEscaping.Always
      };
      result = await kSqlDbRestApiClient.InsertIntoAsync(Tweet1, insertProperties);
      result.IsSuccess().Should().BeTrue();

      result = await kSqlDbRestApiClient.InsertIntoAsync(Tweet2, insertProperties);
      result.IsSuccess().Should().BeTrue();
    }

    [OneTimeTearDown]
    public static async Task ClassCleanup()
    {
      var dropFromItemProperties = new DropFromItemProperties
      {
        IdentifierEscaping = IdentifierEscaping.Always,
        ShouldPluralizeEntityName = false,
        EntityName = StreamName,
        UseIfExistsClause = true,
        DeleteTopic = true,
      };
      await kSqlDbRestApiClient.DropStreamAsync<Tweet>(dropFromItemProperties);
    }

    [SetUp]
    public override void TestInitialize()
    {
      base.TestInitialize();

      Context = CreateKSqlDbContext(ksqlDB.RestApi.Client.KSql.Query.Options.EndpointType.QueryStream, modelBuilder);
    }

    protected virtual IQbservable<Tweet> QuerySource =>
      Context.CreatePushQuery<Tweet>($"`{StreamName}`");

    [Test]
    public async Task Select()
    {
      //Arrange
      int expectedItemsCount = 2;

      var source = QuerySource
        .ToAsyncEnumerable();

      var tweet1 = Tweet1 with { RowTime = 0 };
      var tweet2 = Tweet2 with { RowTime = 0 };

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      var expectedValues = new List<Tweet>
      {
        tweet1, tweet2
      };
      
      expectedItemsCount.Should().Be(actualValues.Count);
      CollectionAssert.AreEqual(expectedValues, actualValues);
    }

    [Test]
    public async Task SelectPseudoColumns()
    {
      //Arrange
      int expectedItemsCount = 2;

      var source = QuerySource
        .Select(c => new{ c.RowTime, c.RowOffset, c.Message })
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      expectedItemsCount.Should().Be(actualValues.Count);
      actualValues[0].RowTime.Should().Be(Tweet1.RowTime);
      actualValues[0].RowTime.Should().BeGreaterOrEqualTo(0);
      actualValues[0].Message.Should().Be(Tweet1.Message);
    }
  }
}
