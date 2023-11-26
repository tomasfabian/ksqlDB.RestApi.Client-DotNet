using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Ninject;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.Tests.KSql.RestApi;

public class KSqlDbQueryProviderTests : TestBase
{
  private TestableKSqlDbQueryProvider ClassUnderTest { get; set; } = null!;

  [SetUp]
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = MockingKernel.Get<TestableKSqlDbQueryProvider>();
  }

  [Test]
  public async Task Run_HttpStatusCodeOK_ReturnsTweets()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var items = ClassUnderTest.Run<Nested>(queryParameters);

    //Assert
    var receivedTweets = new List<Nested>();
    await foreach (var item in items)
    {
      item.Should().NotBeNull();
      receivedTweets.Add(item);
    }

    receivedTweets.Count.Should().Be(2);
  }

  internal struct MovieStruct
  {
    public string Title { get; set; }

    public int Id { get; set; }
  }

  internal class Nested
  {
    public int Id { get; set; }
    public MovieStruct[] Arr { get; set; } = null!;
    public Dictionary<string, Dictionary<string, int>> MapValue { get; set; } = null!;
    public Dictionary<int, string[]> MapArr { get; set; } = null!;
    public MovieStruct Movie { get; set; }
    public int Release_Year { get; set; }
  }
}
