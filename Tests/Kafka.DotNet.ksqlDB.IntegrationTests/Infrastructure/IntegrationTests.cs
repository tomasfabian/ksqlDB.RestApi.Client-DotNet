using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.IntegrationTests.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.Query.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq
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

      ContextOptions = new KSqlDBContextOptions(KSqlDbRestApiProvider.KsqlDbUrl);
      
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