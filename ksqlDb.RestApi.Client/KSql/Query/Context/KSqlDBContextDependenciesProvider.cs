using System;
using System.Linq;
using System.Threading.Tasks;
using ksqlDb.RestApi.Client.DependencyInjection;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.Infrastructure.Logging;
using ksqlDB.RestApi.Client.KSql.Disposables;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  public abstract class KSqlDBContextDependenciesProvider : AsyncDisposableObject
  {
    private readonly KSqlDBContextOptions kSqlDbContextOptions;
    private readonly ILoggerFactory loggerFactory;
    private readonly IServiceCollection serviceCollection;

    protected KSqlDBContextDependenciesProvider(KSqlDBContextOptions kSqlDbContextOptions, ILoggerFactory loggerFactory = null)
      : this()
    {
      this.kSqlDbContextOptions = kSqlDbContextOptions ?? throw new ArgumentNullException(nameof(kSqlDbContextOptions));
      this.loggerFactory = loggerFactory;

      if (kSqlDbContextOptions.ServiceCollection.Any())
        serviceCollection = kSqlDbContextOptions.ServiceCollection;
    }

    protected KSqlDBContextDependenciesProvider()
    {
      serviceCollection = new ServiceCollection();
    }

    protected ServiceProvider ServiceProvider { get; set; }
    
    private bool wasConfigured;

    internal IServiceScopeFactory Initialize(KSqlDBContextOptions contextOptions)
    {
      if (!wasConfigured)
      {
        wasConfigured = true;

        RegisterDependencies(contextOptions);

        ServiceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions {ValidateScopes = true});
      }

      var serviceScopeFactory = ServiceProvider.GetService<IServiceScopeFactory>();

      return serviceScopeFactory;
    }

    private void RegisterDependencies(KSqlDBContextOptions contextOptions)
    {
      OnConfigureServices(serviceCollection, contextOptions);
    }

    internal void Configure(Action<IServiceCollection> receive)
    {
      receive?.Invoke(serviceCollection);
    }

    protected ILogger Logger { get; private set; }

    protected virtual void OnConfigureServices(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
    {
      if (loggerFactory != null)
      {
        serviceCollection.TryAddSingleton(_ => loggerFactory);

        Logger = loggerFactory.CreateLogger(LoggingCategory.Name);

        serviceCollection.TryAddSingleton(Logger);
      }

      serviceCollection.TryAddSingleton<KSqlDbProviderOptions>(contextOptions);
      serviceCollection.TryAddSingleton(contextOptions);

      serviceCollection.TryAddScoped<IKSqlQbservableProvider, QbservableProvider>();
      serviceCollection.TryAddScoped<ICreateStatementProvider, CreateStatementProvider>();

      serviceCollection.TryAddTransient<IKSqlQueryGenerator, KSqlQueryGenerator>();

      serviceCollection.ConfigureHttpClients(contextOptions);

      serviceCollection.TryAddSingleton(contextOptions);
      serviceCollection.TryAddScoped<IKStreamSetDependencies, KStreamSetDependencies>();
      serviceCollection.TryAddScoped<IKSqlDbRestApiClient, KSqlDbRestApiClient>();
    }

    private void RegisterHttpClients(IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
    {
      var uri = new Uri(contextOptions.Url);

      if (!serviceCollection.HasRegistration<IHttpV1ClientFactory>())
      {
        var httpClientV1Builder = serviceCollection.AddHttpClient<IHttpV1ClientFactory, HttpClientFactory>(httpClient =>
        {
          httpClient.BaseAddress = uri;
        });

        if (kSqlDbContextOptions.UseBasicAuth)
        {
          var basicAuthCredentials =
            new BasicAuthCredentials(contextOptions.BasicAuthUserName, contextOptions.BasicAuthPassword);

          httpClientV1Builder.AddHttpMessageHandler(_ => new BasicAuthHandler(basicAuthCredentials));
        }
      }

#if !NETSTANDARD
      if (!serviceCollection.HasRegistration<IHttpClientFactory>())
      {
        var httpClientBuilder = serviceCollection.AddHttpClient<IHttpClientFactory, HttpClientFactory>(httpClient =>
        {
          httpClient.BaseAddress = uri;
          httpClient.DefaultRequestVersion = new Version(2, 0);
        });

        if (kSqlDbContextOptions.UseBasicAuth)
        {
          var basicAuthCredentials =
            new BasicAuthCredentials(contextOptions.BasicAuthUserName, contextOptions.BasicAuthPassword);

          httpClientBuilder.AddHttpMessageHandler(_ => new BasicAuthHandler(basicAuthCredentials));
        }
      }
#endif
    }

    protected override async ValueTask OnDisposeAsync()
    {
      if(ServiceProvider != null)
        await ServiceProvider.DisposeAsync();
    }
  }
}