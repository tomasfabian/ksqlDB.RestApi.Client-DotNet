using FluentAssertions;
using Moq;
using UnitTests;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;
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
    using var subscription = context.CreatePushQuery<string>()
      .Subscribe(_ => { });

    //Assert
    subscription.Should().NotBeNull();
    context.KSqlDbProviderMock.Verify(c => c.Run<string>(It.IsAny<object>(), It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Test]
  public void DependenciesForQueryEndpointTypeWereConfigured()
  {
    //Arrange
    KSqlDBContextOptions contextOptions = new(TestParameters.KsqlDbUrl)
    {
      EndpointType = EndpointType.Query
    };
    var context = new ProtoBufKSqlDbContext(contextOptions);

    _ = context.CreatePushQuery<int>();

    var serviceProvider = context.ServiceCollection
      .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

    //Act
    var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    var queryDbProvider = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbProvider>();
    var pushQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbParameters>();
    var pullQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IPullQueryParameters>();

    //Assert
    queryDbProvider.Should().BeOfType<ProtoBuf.KSql.RestApi.KSqlDbQueryProvider>();
    pushQueryParameters.Should().BeOfType<QueryParameters>();
    pullQueryParameters.Should().BeOfType<PullQueryParameters>();
  }

  [Test]
  public void DependenciesForQueryStreamEndpointTypeWereConfigured()
  {
    //Arrange
    KSqlDBContextOptions contextOptions = new(TestParameters.KsqlDbUrl)
    {
      EndpointType = EndpointType.QueryStream
    };
    var context = new ProtoBufKSqlDbContext(contextOptions);

    _ = context.CreatePushQuery<int>();

    var serviceProvider = context.ServiceCollection
      .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

    //Act
    var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    var queryDbProvider = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbProvider>();
    var pushQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IKSqlDbParameters>();
    var pullQueryParameters = serviceScopeFactory.CreateScope().ServiceProvider.GetService<IPullQueryParameters>();

    //Assert
    queryDbProvider.Should().BeOfType<ProtoBuf.KSql.RestApi.KSqlDbQueryStreamProvider>();
    pushQueryParameters.Should().BeOfType<QueryStreamParameters>();
    pullQueryParameters.Should().BeOfType<PullQueryStreamParameters>();
  }
}
