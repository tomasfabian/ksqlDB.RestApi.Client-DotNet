using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDb.RestApi.Client.IntegrationTests.Infrastructure;

[TestClass]
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

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    Context = CreateKSqlDbContext(EndpointType.QueryStream);
  }

  protected KSqlDBContext CreateKSqlDbContext(EndpointType endpointType)
  {
    ContextOptions = new KSqlDBContextOptions(KSqlDbRestApiProvider.KsqlDbUrl)
    {
      ShouldPluralizeFromItemName = false,
      EndpointType = endpointType
    };

    return new KSqlDBContext(ContextOptions);
  }

  [TestCleanup]
  public override void TestCleanup()
  {
    Context.DisposeAsync().GetAwaiter().GetResult();

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
