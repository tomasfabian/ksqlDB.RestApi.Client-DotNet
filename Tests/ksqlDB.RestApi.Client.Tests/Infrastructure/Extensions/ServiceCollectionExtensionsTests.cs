using FluentAssertions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi;

namespace ksqlDb.RestApi.Client.Tests.Infrastructure.Extensions
{
  public class ServiceCollectionExtensionsTests
  {
    private readonly IServiceCollection serviceCollection = new ServiceCollection();

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

      //Act
      serviceCollection.ConfigureHttpClients(new KSqlDBContextOptions(Helpers.TestParameters.KsqlDbUrl));

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

      //Assert
      httpClientFactory.Should().NotBeNull();
    }
  }
}
