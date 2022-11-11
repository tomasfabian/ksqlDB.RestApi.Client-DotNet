using FluentAssertions;
using ksqlDb.RestApi.Client.ProtoBuf.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests;
using ksqlDB.RestApi.Client.KSql.Linq;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.Query;

[TestClass]
public class ProtoBufKSqlDbContextTests : TestBase
{
  [TestMethod]
  public void CreateQueryStream_Subscribe_KSqlDbProvidersRunWasCalled()
  {
    //Arrange
    var context = new TestableProtoBufKSqlDbContext<string>(TestParameters.KsqlDbUrl);

    //Act
    using var subscription = context.CreateQueryStream<string>()
      .Subscribe(_ => { });

    //Assert
    subscription.Should().NotBeNull();
    context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()),
      Times.Once);
  }
}