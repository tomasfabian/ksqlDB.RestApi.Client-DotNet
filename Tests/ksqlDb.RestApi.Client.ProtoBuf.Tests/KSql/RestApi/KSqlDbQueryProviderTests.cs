using FluentAssertions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDb.RestApi.Client.ProtoBuf.Tests.Models;
using Ninject;
using NUnit.Framework;
using UnitTests;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.RestApi;

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
  public async Task Run_RetrievesDeserializedValues()
  {
    //Arrange
    var queryParameters = new QueryStreamParameters();

    //Act
    var tweets = await ClassUnderTest.Run<MovieProto>(queryParameters).ToListAsync();

    //Assert
    tweets.Count.Should().Be(2);
  }
}
