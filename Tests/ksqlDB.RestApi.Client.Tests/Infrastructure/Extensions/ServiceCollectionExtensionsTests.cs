using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

namespace ksqlDb.RestApi.Client.Tests.Infrastructure.Extensions
{
  public class ServiceCollectionExtensionsTests
  {
    private IServiceCollection serviceCollection;

    [SetUp]
    public void Setup()
    {
      serviceCollection = new ServiceCollection();
    }

    [Test]
    public void RegisterKSqlDbContextDependencies()
    {
      //Arrange
      var modelBuilder = new ModelBuilder();
      serviceCollection.AddSingleton<IMetadataProvider>(modelBuilder);

      var restApiClientOptions = new KSqlDBRestApiClientOptions
      {
        ShouldPluralizeFromItemName = false,
      };
      serviceCollection.AddSingleton(restApiClientOptions);

      //Act
      serviceCollection.RegisterKSqlDbContextDependencies(new KSqlDBContextOptions(Helpers.TestParameters.KsqlDbUrl));

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var metadataProvider = serviceProvider.GetRequiredService<IMetadataProvider>();
      var clientOptions = serviceProvider.GetRequiredService<KSqlDBRestApiClientOptions>();
      var rRestApiClient = serviceProvider.GetRequiredService<IKSqlDbRestApiClient>();

      //Assert
      metadataProvider.Should().BeSameAs(modelBuilder);
      restApiClientOptions.Should().BeSameAs(clientOptions);
      rRestApiClient.Should().NotBeNull();
    }

    [Test]
    public void ConfigureHttpClients()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(Helpers.TestParameters.KsqlDbUrl);

      //Act
      serviceCollection.ConfigureHttpClients(contextOptions);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var httpClientFactory = serviceProvider.GetRequiredService<ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory>();

      //Assert
      httpClientFactory.Should().NotBeNull();

      var httpClient = httpClientFactory.CreateClient();
      httpClient.BaseAddress.Should().Be(Helpers.TestParameters.KsqlDbUrl);
    }

    [Test]
    public void RegisterEndpointDependencies()
    {
      //Arrange
      var contextOptions = new KSqlDBContextOptions(Helpers.TestParameters.KsqlDbUrl);

      //Act
      serviceCollection.RegisterEndpointDependencies(contextOptions);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var dbParameters = serviceProvider.GetRequiredService<IKSqlDbParameters>();
      var pullQueryParameters = serviceProvider.GetRequiredService<IPullQueryParameters>();

      //Assert
      dbParameters.Should().NotBeNull();
      pullQueryParameters.Should().NotBeNull();
    }

    [Test]
    public void ApplyTo()
    {
      //Arrange
      var sc = new ServiceCollection();
      var restApiClientOptions = new KSqlDBRestApiClientOptions
      {
        ShouldPluralizeFromItemName = false,
      };
      sc.AddSingleton(restApiClientOptions);

      //Act
      sc.ApplyTo(serviceCollection);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var receivedRestApiClientOptions = serviceProvider.GetRequiredService<KSqlDBRestApiClientOptions>();

      //Assert
      receivedRestApiClientOptions.Should().NotBeNull();
      receivedRestApiClientOptions.ShouldPluralizeFromItemName.Should().BeFalse();
    }
  }
}
