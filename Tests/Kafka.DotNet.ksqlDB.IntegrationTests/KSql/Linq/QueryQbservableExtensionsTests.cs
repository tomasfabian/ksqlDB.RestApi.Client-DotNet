using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.IntegrationTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kafka.DotNet.ksqlDB.IntegrationTests.KSql.Linq
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

    protected override ksqlDB.KSql.Linq.IQbservable<Tweet> QuerySource =>
      Context.CreateQuery<Tweet>(StreamName);
  }
}