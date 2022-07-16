using System;
using System.Net.Http;
using FluentAssertions;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.Api.Client.Tests.DependencyInjection;

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
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);

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
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl, setupParameters =>
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
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl, setupParameters =>
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
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDbRestApiClient>();
        
    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [TestMethod]
  public void ConfigureKSqlDb_BuildServiceProviderAndResolve_IKSqlDbRestApiClient()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);

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
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<IHttpClientFactory>();
        
    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
  }

  [TestMethod]
  public void ConfigureKSqlDb_KSqlDBContextOptions()
  {
    //Arrange

    //Act
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<KSqlDBContextOptions>();
        
    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
  }

  [TestMethod]
  public void ConfigureKSqlDb_BuildServiceProviderAndResolve_IHttpClientFactory()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var httpClientFactory = ClassUnderTest.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    //Assert
    httpClientFactory.Should().NotBeNull();
  }

  [TestMethod]
  public void ConfigureKSqlDb_BuildServiceProviderAndResolve_KSqlDBContextOptions()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);

    //Act
    var options = ClassUnderTest.BuildServiceProvider().GetRequiredService<KSqlDBContextOptions>();

    //Assert
    options.Should().NotBeNull();
    options.Url.Should().Be(TestParameters.KsqlDBUrl);
  }

  #region AddDbContext

  [TestMethod]
  public void AddDbContext_RegisterAsInterface()
  {
    //Arrange
    ClassUnderTest.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl), ServiceLifetime.Transient);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>();

    //Assert
    context.Should().NotBeNull();
  }
    
  [TestMethod]
  public void AddDbContext_KSqlDBContext_DefaultLifetimeIsScoped()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl));

    //Act
    var descriptor = ClassUnderTest.TryGetRegistration<KSqlDBContext>();

    //Assert
    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }
    
  [TestMethod]
  public void AddDbContext_KSqlDBContext_ContextLifetimeChangedToTransientScope()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl), contextLifetime: ServiceLifetime.Transient);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<KSqlDBContext>();

    //Assert
    context.Should().NotBeNull();

    var descriptor = ClassUnderTest.TryGetRegistration<KSqlDBContext>();

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
  }


  [TestMethod]
  public void AddDbContext_IKSqlDbRestApiClient_DefaultLifetimeIsScoped()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl));

    //Act
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDbRestApiClient>();

    //Assert
    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [TestMethod]
  public void AddDbContext_RestApiLifetimeChangedToTransientScope()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl), restApiLifetime: ServiceLifetime.Transient);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDbRestApiClient>();

    //Assert
    context.Should().NotBeNull();

    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDbRestApiClient>();

    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
  }
    
  #endregion
    
  #region ContextFactory

  [TestMethod]
  [ExpectedException(typeof(InvalidOperationException))]
  public void AddDbContextFactory_DbContextWasNotRegistered_Throws()
  {
    //Arrange
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>();

    //Assert
  }

  [TestMethod]
  public void ConfigureKSqlDb_AddDbContextFactory_DbContextWasRegistered()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDBUrl);
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>();

    //Assert
    context.Should().NotBeNull();
  }

  [TestMethod]
  public void AddDbContextFactory_BuildServiceProviderAndResolve()
  {
    //Arrange
    ClassUnderTest.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl), ServiceLifetime.Transient);
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Act
    var contextFactory = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

    //Assert
    contextFactory.Should().NotBeNull();
  }

  [TestMethod]
  public void ContextFactory_Create()
  {
    //Arrange
    ClassUnderTest.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl), ServiceLifetime.Transient);
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    var contextFactory = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

    //Act
    var context1 = contextFactory.Create();
    var context2 = contextFactory.Create();

    //Assert
    context1.Should().NotBeNull();
    context1.Should().NotBeSameAs(context2);
  }

  [TestMethod]
  public void AddDbContextFactory_Scope()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDBUrl), ServiceLifetime.Transient);
    ClassUnderTest.AddDbContextFactory<KSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Act
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDBContextFactory<KSqlDBContext>>();

    //Assert
    descriptor.Should().NotBeNull();
    descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  #endregion

  [TestMethod]
  public void ReplaceHttpClient_HttpClientUriAndTimeoutWereSet()
  {
    //Arrange
    var services = new ServiceCollection();

    services.AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
    {
      c.UseKSqlDb(TestParameters.KsqlDBUrl);

      c.ReplaceHttpClient<IHttpClientFactory, HttpClientFactory>(httpClient =>
      {
        httpClient.Timeout = TimeSpan.FromMinutes(5);
      }).AddHttpMessageHandler(_ => Mock.Of<DelegatingHandler>());
    });

    var provider = services.BuildServiceProvider();

    //Act
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();

    //Assert
    httpClient.Timeout.Should().Be(TimeSpan.FromMinutes(5));
    httpClient.BaseAddress!.OriginalString.Should().Be(TestParameters.KsqlDBUrl);
  }
}