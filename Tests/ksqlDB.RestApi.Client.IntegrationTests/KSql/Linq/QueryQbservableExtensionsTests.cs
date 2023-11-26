using ksqlDb.RestApi.Client.IntegrationTests.Models;
using ksqlDB.RestApi.Client.KSql.Linq;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq;

public class QueryQbservableExtensionsTests : QbservableExtensionsTests
{
  [OneTimeSetUp]
  public new static async Task ClassInitialize()
  {
    await InitializeDatabase();
  }
    
  [OneTimeTearDown]
  public new static async Task ClassCleanup()
  {
    var result = await RestApiProvider.DropStreamAndTopic(StreamName);
  }

  protected override IQbservable<Tweet> QuerySource =>
    Context.CreateQuery<Tweet>(StreamName);
}
