using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ksqlDB.Api.Client.IntegrationTests.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.IntegrationTests.Infrastructure
{
  [TestClass]
  public abstract class IntegrationTests : TestBase
  {
    protected static KSqlDbRestApiProvider RestApiProvider;  
    protected KSqlDBContextOptions ContextOptions;
    protected KSqlDBContext Context;

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ContextOptions = new KSqlDBContextOptions(KSqlDbRestApiProvider.KsqlDbUrl)
      {
        ShouldPluralizeFromItemName = false
      };
      
      Context = new KSqlDBContext(ContextOptions);
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
}