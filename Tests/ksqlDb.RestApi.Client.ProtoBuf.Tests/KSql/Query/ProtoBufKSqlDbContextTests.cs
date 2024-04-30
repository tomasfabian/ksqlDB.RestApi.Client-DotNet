using FluentAssertions;
using Moq;
using UnitTests;
using ksqlDB.RestApi.Client.KSql.Linq;
using NUnit.Framework;
using TestParameters = ksqlDb.RestApi.Client.ProtoBuf.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.Query;

public class ProtoBufKSqlDbContextTests : TestBase
{
  [Test]
  public void CreatePushQuery_Subscribe_KSqlDbProvidersRunWasCalled()
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
