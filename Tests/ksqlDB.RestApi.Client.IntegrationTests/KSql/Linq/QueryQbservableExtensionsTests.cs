using System.Threading.Tasks;
using ksqlDB.Api.Client.IntegrationTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ksqlDB.Api.Client.IntegrationTests.KSql.Linq
{
  [TestClass]
  public class QueryQbservableExtensionsTests : QbservableExtensionsTests
  {
    [ClassInitialize]
    public new static async Task ClassInitialize(TestContext context)
    {
      await InitializeDatabase();
    }
    
    [ClassCleanup]
    public new static async Task ClassCleanup()
    {
      var result = await RestApiProvider.DropStreamAndTopic(StreamName);
    }

    protected override Kafka.DotNet.ksqlDB.KSql.Linq.IQbservable<Tweet> QuerySource =>
      Context.CreateQuery<Tweet>(StreamName);
  }
}