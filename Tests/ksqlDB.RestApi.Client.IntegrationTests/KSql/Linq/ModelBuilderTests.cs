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
    };

    public static readonly Tweet Tweet2 = new()
    {
      Id = 2,
      Message = "Wall-e",
      IsRobot = false,
      Amount = 1,
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
      await kSqlDbRestApiClient.DropStreamAsync<Models.Tweet>(dropFromItemProperties);
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

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      var expectedValues = new List<Tweet>
      {
        Tweet1, Tweet2
      };
      
      expectedItemsCount.Should().Be(actualValues.Count);
      CollectionAssert.AreEqual(expectedValues, actualValues);
    }
  }
}
