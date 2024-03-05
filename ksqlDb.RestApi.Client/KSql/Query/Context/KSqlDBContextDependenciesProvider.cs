using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.Infrastructure.Logging;
using ksqlDB.RestApi.Client.KSql.Disposables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ksqlDB.RestApi.Client.KSql.Query.Context;

/// <summary>
/// Dependencies provider for ksqlDB context queries using REST API.
/// </summary>
public abstract class KSqlDBContextDependenciesProvider : AsyncDisposableObject, IDisposable
{
  private readonly ILoggerFactory loggerFactory;

  /// <summary>
  /// Initializes a new instance of <see cref="KSqlDBContextDependenciesProvider"/> with the specified <see cref="KSqlDBContextOptions"/>. 
  /// </summary>
  /// <param name="kSqlDbContextOptions">The options for ksqlDB context.</param>
  /// <param name="loggerFactory">The logger factory.</param>
  protected KSqlDBContextDependenciesProvider(KSqlDBContextOptions kSqlDbContextOptions, ILoggerFactory loggerFactory = null)
  {
    if (kSqlDbContextOptions == null) throw new ArgumentNullException(nameof(kSqlDbContextOptions));
    this.loggerFactory = loggerFactory;

    ServiceCollection = new ServiceCollection();
    ServiceCollection = kSqlDbContextOptions.ServiceCollection;
  }

  internal IServiceCollection ServiceCollection { get; }

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
