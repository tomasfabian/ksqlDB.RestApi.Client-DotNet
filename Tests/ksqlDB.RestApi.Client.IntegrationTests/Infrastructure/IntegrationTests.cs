using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.IntegrationTests.Infrastructure;

[TestFixture]
public abstract class IntegrationTests : TestBase
{
  protected static KSqlDbRestApiProvider RestApiProvider = null!;
  protected KSqlDBContextOptions ContextOptions = null!;
  private KSqlDBContext? context;

  protected KSqlDBContext Context
  {
    get => context ??= (CreateKSqlDbContext(EndpointType.QueryStream));
    set
    {
      context?.Dispose();

      context = value;
    }
  }

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    Context = CreateKSqlDbContext(EndpointType.QueryStream);
  }

  protected KSqlDBContext CreateKSqlDbContext(EndpointType endpointType, ModelBuilder? modelBuilder = null)
  {
    ContextOptions = new KSqlDBContextOptions(KSqlDbRestApiProvider.KsqlDbUrl)
    {
      ShouldPluralizeFromItemName = false,
      EndpointType = endpointType
    };

    modelBuilder ??= new ModelBuilder();
    return new KSqlDBContext(ContextOptions, modelBuilder);
  }

  [TearDown]
  public override void TestCleanup()
  {
    Context.DisposeAsync().GetAwaiter().GetResult();
    Context.Dispose();

    base.TestCleanup();
  }

  protected static async Task<List<T>> CollectActualValues<T>(IAsyncEnumerable<T> source, int? expectedItemsCount = null)
  {
    var actualValues = new List<T>();

    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(15));

    if (expectedItemsCount.HasValue)
      source = source.Take(expectedItemsCount.Value);

    await foreach (var item in source.WithCancellation(cts.Token))
    {
      actualValues.Add(item);
    }

    return actualValues;
  }
}
