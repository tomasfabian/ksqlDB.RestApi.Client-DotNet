using FluentAssertions;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace ksqlDB.Api.Client.Tests.DependencyInjection
{
  [TestClass]
  public class KSqlDbServiceCollectionExtensionsTests : TestBase
  {
    private ServiceCollection ClassUnderTest { get; set; }

    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();

      ClassUnderTest = new ServiceCollection();
    }

    [TestMethod]
    public void ConfigureKSqlDb_IKSqlDBContext()
    {
      //Arrange

      //Act
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl);

      //Assert
      var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDBContext>();
        
      descriptor.Should().NotBeNull();
      descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [TestMethod]
    public void ConfigureKSqlDb_SetupParametersAction()
    {
      //Arrange

      //Act
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl, setupParameters =>
                                                                       {
                                                                         setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);
                                                                       });

      //Assert
      var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDBContext>();
        
      descriptor.Should().NotBeNull();
      descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [TestMethod]
    public void ConfigureKSqlDb_BuildServiceProviderAndResolve()
    {
      //Arrange
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl, setupParameters =>
                                                                       {
                                                                         setupParameters.SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce);
                                                                       });

      //Act
      var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>() as KSqlDBContext;

      //Assert
      context.Should().NotBeNull();
      context.ContextOptions.QueryStreamParameters[KSqlDbConfigs.ProcessingGuarantee].ToProcessingGuarantee().Should().Be(ProcessingGuarantee.AtLeastOnce);
    }

    [TestMethod]
    public void ConfigureKSqlDb_IKSqlDbRestApiClient()
    {
      //Arrange

      //Act
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl);

      //Assert
      var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDbRestApiClient>();
        
      descriptor.Should().NotBeNull();
      descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [TestMethod]
    public void ConfigureKSqlDb_BuildServiceProviderAndResolve_IKSqlDbRestApiClient()
    {
      //Arrange
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl);

      //Act
      var kSqlDbRestApiClient = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDbRestApiClient>();

      //Assert
      kSqlDbRestApiClient.Should().NotBeNull();
    }

    [TestMethod]
    public void ConfigureKSqlDb_IHttpClientFactory()
    {
      //Arrange

      //Act
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl);

      //Assert
      var descriptor = ClassUnderTest.TryGetRegistration<IHttpClientFactory>();
        
      descriptor.Should().NotBeNull();
      descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [TestMethod]
    public void ConfigureKSqlDb_KSqlDBContextOptions()
    {
      //Arrange

      //Act
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl);

      //Assert
      var descriptor = ClassUnderTest.TryGetRegistration<KSqlDBContextOptions>();
        
      descriptor.Should().NotBeNull();
      descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [TestMethod]
    public void ConfigureKSqlDb_BuildServiceProviderAndResolve_IHttpClientFactory()
    {
      //Arrange
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl);

      //Act
      var httpClientFactory = ClassUnderTest.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

      //Assert
      httpClientFactory.Should().NotBeNull();
    }

    [TestMethod]
    public void ConfigureKSqlDb_BuildServiceProviderAndResolve_KSqlDBContextOptions()
    {
      //Arrange
      ClassUnderTest.ConfigureKSqlDb(Helpers.TestParameters.KsqlDBUrl);

      //Act
      var options = ClassUnderTest.BuildServiceProvider().GetRequiredService<KSqlDBContextOptions>();

      //Assert
      options.Should().NotBeNull();
      options.Url.Should().Be(Helpers.TestParameters.KsqlDBUrl);
    }
  }
}