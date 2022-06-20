using System;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.Infrastructure.Logging;
using ksqlDB.RestApi.Client.KSql.Disposables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  public abstract class KSqlDBContextDependenciesProvider : AsyncDisposableObject, IDisposable
  {
    private readonly KSqlDBContextOptions kSqlDbContextOptions;
    private readonly ILoggerFactory loggerFactory;

    protected KSqlDBContextDependenciesProvider(KSqlDBContextOptions kSqlDbContextOptions, ILoggerFactory loggerFactory = null)
    {
      this.kSqlDbContextOptions = kSqlDbContextOptions ?? throw new ArgumentNullException(nameof(kSqlDbContextOptions));
      this.loggerFactory = loggerFactory;
    }

    protected IServiceCollection ServiceCollection => kSqlDbContextOptions.ServiceCollection;

    protected ServiceProvider ServiceProvider { get; private set; }

    private bool wasConfigured;

    internal IServiceScopeFactory Initialize(KSqlDBContextOptions contextOptions)
    {
      if (!wasConfigured)
      {
        wasConfigured = true;

        RegisterDependencies(contextOptions);

        ServiceProvider = ServiceCollection.BuildServiceProvider(new ServiceProviderOptions {ValidateScopes = true});
      }

      var serviceScopeFactory = ServiceProvider.GetService<IServiceScopeFactory>();

      return serviceScopeFactory;
    }

    private void RegisterDependencies(KSqlDBContextOptions contextOptions)
    {
      OnConfigureServices(ServiceCollection, contextOptions);
    }

    internal void Configure(Action<IServiceCollection> receive)
    {
      receive?.Invoke(ServiceCollection);
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

      serviceCollection.RegisterKSqlDbContextDependencies(contextOptions);
    }

    protected override async ValueTask OnDisposeAsync()
    {
      if(ServiceProvider != null)
        await ServiceProvider.DisposeAsync().ConfigureAwait(false);
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing) return;

      ServiceProvider?.Dispose();
      ServiceProvider = null;
    }
  }
}