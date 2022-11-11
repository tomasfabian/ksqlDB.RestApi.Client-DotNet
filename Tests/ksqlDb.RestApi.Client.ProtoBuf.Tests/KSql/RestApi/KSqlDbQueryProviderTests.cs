using FluentAssertions;
using ksqlDb.RestApi.Client.ProtoBuf.Tests.Models;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using UnitTests;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.RestApi;

[TestClass]
public class KSqlDbQueryProviderTests : TestBase
{  
  private TestableKSqlDbQueryProvider ClassUnderTest { get; set; }

  [TestInitialize]
  public override void TestInitialize()
  {
    base.TestInitialize();

    ClassUnderTest = MockingKernel.Get<TestableKSqlDbQueryProvider>();
  }

  [TestMethod]
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