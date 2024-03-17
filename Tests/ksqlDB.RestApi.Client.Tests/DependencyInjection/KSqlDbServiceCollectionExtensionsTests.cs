using FluentAssertions;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Config;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;
using TestParameters = ksqlDb.RestApi.Client.Tests.Helpers.TestParameters;

namespace ksqlDb.RestApi.Client.Tests.DependencyInjection;

public class KSqlDbServiceCollectionExtensionsTests
{
  private ServiceCollection ClassUnderTest { get; set; } = null!;

  [SetUp]
  public void Setup()
  {
    ClassUnderTest = new ServiceCollection();
  }

  [Test]
  public void ConfigureKSqlDb_IKSqlDBContext()
  {
    //Arrange

    //Act
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDBContext>();
        
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Test]
  public void ConfigureKSqlDb_SetupParametersAction()
  {
    //Arrange

    //Act
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl, setupParameters =>
    {
      setupParameters.SetAutoOffsetReset(AutoOffsetReset.Earliest);
    });

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDBContext>();
        
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Test]
  public void ConfigureKSqlDb_BuildServiceProviderAndResolve()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl, setupParameters =>
    {
      setupParameters.SetProcessingGuarantee(ProcessingGuarantee.AtLeastOnce);
    });

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>() as KSqlDBContext;

    //Assert
    context.Should().NotBeNull();
    context?.ContextOptions.QueryStreamParameters[KSqlDbConfigs.ProcessingGuarantee].ToProcessingGuarantee().Should().Be(ProcessingGuarantee.AtLeastOnce);
  }

  [Test]
  public void ConfigureKSqlDb_IKSqlDbRestApiClient()
  {
    //Arrange

    //Act
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDbRestApiClient>();
        
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Test]
  public void ConfigureKSqlDb_BuildServiceProviderAndResolve_IKSqlDbRestApiClient()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var kSqlDbRestApiClient = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDbRestApiClient>();

    //Assert
    kSqlDbRestApiClient.Should().NotBeNull();
  }

  [Test]
  public void ConfigureKSqlDb_IHttpClientFactory()
  {
    //Arrange

    //Act
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<IHttpClientFactory>();
        
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
  }

  [Test]
  public void ConfigureKSqlDb_KSqlDBContextOptions()
  {
    //Arrange

    //Act
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);

    //Assert
    var descriptor = ClassUnderTest.TryGetRegistration<KSqlDBContextOptions>();
        
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
  }

  [Test]
  public void ConfigureKSqlDb_BuildServiceProviderAndResolve_IHttpClientFactory()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var httpClientFactory = ClassUnderTest.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    //Assert
    httpClientFactory.Should().NotBeNull();
  }

  [Test]
  public void ConfigureKSqlDb_BuildServiceProviderAndResolve_KSqlDBContextOptions()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);

    //Act
    var options = ClassUnderTest.BuildServiceProvider().GetRequiredService<KSqlDBContextOptions>();

    //Assert
    options.Should().NotBeNull();
    options.Url.Should().Be(TestParameters.KsqlDbUrl);
  }

  #region AddDbContext

  [Test]
  public void AddDbContext_RegisterAsInterface()
  {
    //Arrange
    ClassUnderTest.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl), ServiceLifetime.Transient);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>();

    //Assert
    context.Should().NotBeNull();
  }
    
  [Test]
  public void AddDbContext_KSqlDBContext_DefaultLifetimeIsScoped()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl));

    //Act
    var descriptor = ClassUnderTest.TryGetRegistration<KSqlDBContext>();

    //Assert
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }
    
  [Test]
  public void AddDbContext_KSqlDBContext_ContextLifetimeChangedToTransientScope()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl), contextLifetime: ServiceLifetime.Transient);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<KSqlDBContext>();

    //Assert
    context.Should().NotBeNull();

    var descriptor = ClassUnderTest.TryGetRegistration<KSqlDBContext>();

    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
  }


  [Test]
  public void AddDbContext_IKSqlDbRestApiClient_DefaultLifetimeIsScoped()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl));

    //Act
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDbRestApiClient>();

    //Assert
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  [Test]
  public void AddDbContext_RestApiLifetimeChangedToTransientScope()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl), restApiLifetime: ServiceLifetime.Transient);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDbRestApiClient>();

    //Assert
    context.Should().NotBeNull();

    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDbRestApiClient>();

    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
  }
    
  #endregion
    
  #region ContextFactory

  [Test]
  public void AddDbContextFactory_DbContextWasNotRegistered_Throws()
  {
    //Arrange
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Assert
    Assert.Throws<InvalidOperationException>(() =>
    {
      //Act
      ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>();
    });
  }

  [Test]
  public void ConfigureKSqlDb_AddDbContextFactory_DbContextWasRegistered()
  {
    //Arrange
    ClassUnderTest.ConfigureKSqlDb(TestParameters.KsqlDbUrl);
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Act
    var context = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContext>();

    //Assert
    context.Should().NotBeNull();
  }

  [Test]
  public void AddDbContextFactory_BuildServiceProviderAndResolve()
  {
    //Arrange
    ClassUnderTest.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl), ServiceLifetime.Transient);
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Act
    var contextFactory = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

    //Assert
    contextFactory.Should().NotBeNull();
  }

  [Test]
  public void ContextFactory_Create()
  {
    //Arrange
    ClassUnderTest.AddDbContext<IKSqlDBContext, KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl), ServiceLifetime.Transient);
    ClassUnderTest.AddDbContextFactory<IKSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    var contextFactory = ClassUnderTest.BuildServiceProvider().GetRequiredService<IKSqlDBContextFactory<IKSqlDBContext>>();

    //Act
    var context1 = contextFactory.Create();
    var context2 = contextFactory.Create();

    //Assert
    context1.Should().NotBeNull();
    context1.Should().NotBeSameAs(context2);
  }

  [Test]
  public void AddDbContextFactory_Scope()
  {
    //Arrange
    ClassUnderTest.AddDbContext<KSqlDBContext>(options => options.UseKSqlDb(TestParameters.KsqlDbUrl), ServiceLifetime.Transient);
    ClassUnderTest.AddDbContextFactory<KSqlDBContext>(factoryLifetime: ServiceLifetime.Scoped);

    //Act
    var descriptor = ClassUnderTest.TryGetRegistration<IKSqlDBContextFactory<KSqlDBContext>>();

    //Assert
    descriptor.Should().NotBeNull();
    descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
  }

  #endregion

  [Test]
  public void ReplaceHttpClient_HttpClientUriAndTimeoutWereSet()
  {
    //Arrange
    var services = new ServiceCollection();

    services.AddDbContext<IKSqlDBContext, KSqlDBContext>(c =>
    {
      c.UseKSqlDb(TestParameters.KsqlDbUrl);

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
    httpClient.BaseAddress!.OriginalString.Should().Be(TestParameters.KsqlDbUrl);
  }
}
