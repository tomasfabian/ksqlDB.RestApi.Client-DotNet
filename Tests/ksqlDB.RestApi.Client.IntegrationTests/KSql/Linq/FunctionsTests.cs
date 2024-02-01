using FluentAssertions;
using ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;
using ksqlDb.RestApi.Client.IntegrationTests.Models;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using NUnit.Framework;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.Linq
{
  public class FunctionsTests : Infrastructure.IntegrationTests
  {
    private static TweetsProvider? tweetsProvider;

    private static readonly string TweetsTopicName = "tweetsFunctionTestTopic";
    protected static string TweetsStreamName = "tweetsFunctionTest";

    [OneTimeSetUp]
    public static async Task ClassInitialize()
    {
      RestApiProvider = KSqlDbRestApiProvider.Create();

      tweetsProvider = new TweetsProvider(RestApiProvider);

      await tweetsProvider.CreateTweetsStream(TweetsStreamName, TweetsTopicName);

      await tweetsProvider.InsertTweetAsync(TweetsProvider.Tweet1, TweetsStreamName);
      await tweetsProvider.InsertTweetAsync(TweetsProvider.Tweet2, TweetsStreamName);
    }

    [OneTimeTearDown]
    public static async Task ClassCleanup()
    {
      await RestApiProvider.DropStreamAndTopic(TweetsStreamName);

      tweetsProvider = null;
    }

    [Test]
    public async Task Explode()
    {
      await Explode(Context.CreateQueryStream<Tweet>(TweetsStreamName));
    }

    private async Task Explode(IQbservable<Tweet> querySource)
    {
      //Arrange
      int expectedItemsCount = 3;

      string[] array1 = ["a", "b"];
      int[] array2 = [1, 2, 3];

      var source = querySource
        .Select(l => new { Result = array1.Explode(), Result1 = array2.Explode() })
        .ToAsyncEnumerable();

      //Act
      var actualValues = await CollectActualValues(source, expectedItemsCount);

      //Assert
      var results = actualValues.FirstOrDefault(c => c.Result == array1[0] && c.Result1 == array2[0]);
      results.Should().NotBeNull();
    }
  }
}
