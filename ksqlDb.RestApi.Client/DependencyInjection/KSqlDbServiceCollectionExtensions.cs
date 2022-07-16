using System;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDb.RestApi.Client.DependencyInjection;

public static class KSqlDbServiceCollectionExtensions
{
  /// <summary>
  /// Registers the given ksqldb context factory as a service in the <see cref="IServiceCollection" />.
  /// </summary>
  /// <typeparam name="TContext">The type of context factory to be registered.</typeparam>
  /// <param name="services">The IServiceCollection to add services to.</param>
  /// <param name="factoryLifetime">The lifetime with which to register the ksqldb context factory service in the container.</param>
  /// <returns>The original IServiceCollection.</returns>
  public static IServiceCollection AddDbContextFactory<TContext>(this IServiceCollection services, ServiceLifetime factoryLifetime)
    where TContext : IKSqlDBContext
  {
    var contextFactoryDescriptor = new ServiceDescriptor(
      typeof(IKSqlDBContextFactory<TContext>),
      typeof(KSqlDBContextFactory<TContext>),
      factoryLifetime);
      
    services.Add(contextFactoryDescriptor);

    return services;
  }

  /// <summary>
  /// Registers the given ksqldb context as a service in the <see cref="IServiceCollection" />.
  /// </summary>
  /// <typeparam name="TContext">The type of context to be registered.</typeparam>
  /// <param name="services">The IServiceCollection to add services to.</param>
  /// <param name="optionsAction">Action to configure the KSqlDbContextOptions for the context.</param>
  /// <param name="contextLifetime">The lifetime with which to register the TContext service in the container.</param>
  /// <param name="restApiLifetime">The lifetime with which to register the IKSqlDbRestApiClient service in the container.</param>
  /// <returns>The original IServiceCollection.</returns>
  public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services,
    Action<KSqlDbContextOptionsBuilder> optionsAction,
    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
    ServiceLifetime restApiLifetime = ServiceLifetime.Scoped)
    where TContext : IKSqlDBContext
  {
    if (optionsAction == null) throw new ArgumentNullException(nameof(optionsAction));

    return services.AddDbContext<TContext, TContext>(optionsAction, contextLifetime, restApiLifetime);
  }

  /// <summary>
  /// Registers the given ksqldb context as a service in the <see cref="IServiceCollection" />.
  /// </summary>
  /// <typeparam name="TContextService">The type of context to be registered as.</typeparam>
  /// <typeparam name="TContextImplementation">The implementation type of the context to be registered.</typeparam>
  /// <param name="services">The IServiceCollection to add services to.</param>
  /// <param name="optionsAction">Action to configure the KSqlDbContextOptions for the context.</param>
  /// <param name="contextLifetime">The lifetime with which to register the TContext service in the container.</param>
  /// <param name="restApiLifetime">The lifetime with which to register the IKSqlDbRestApiClient service in the container.</param>
  /// <returns>The original IServiceCollection.</returns>
  public static IServiceCollection AddDbContext<TContextService, TContextImplementation>(this IServiceCollection services,
    Action<KSqlDbContextOptionsBuilder> optionsAction,
    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
    ServiceLifetime restApiLifetime = ServiceLifetime.Scoped)
    where TContextService : IKSqlDBContext
    where TContextImplementation : IKSqlDBContext
  {
    if (optionsAction == null) throw new ArgumentNullException(nameof(optionsAction));

    var builder = new KSqlDbContextOptionsBuilder();

    optionsAction(builder);

    services.ConfigureKSqlDb<TContextService, TContextImplementation>(builder, contextLifetime, restApiLifetime);

    return services;
  }

  internal static IServiceCollection ConfigureKSqlDb<TContextService, TContextImplementation>(this IServiceCollection services,
    KSqlDbContextOptionsBuilder builder,
    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
    ServiceLifetime restApiLifetime = ServiceLifetime.Scoped)
    where TContextService : IKSqlDBContext
    where TContextImplementation : IKSqlDBContext
  {
    var contextOptions = builder.InternalOptions;

    contextOptions.ServiceCollection.AddSingleton(contextOptions);

    var contextServiceDescriptor = new ServiceDescriptor(
      typeof(TContextService),
      typeof(TContextImplementation),
      contextLifetime);

    contextOptions.ServiceCollection.Add(contextServiceDescriptor);

    contextOptions.ServiceCollection.ConfigureHttpClients(contextOptions);

    var restApiServiceDescriptor = new ServiceDescriptor(
      typeof(IKSqlDbRestApiClient),
      typeof(KSqlDbRestApiClient),
      restApiLifetime);

    contextOptions.ServiceCollection.Add(restApiServiceDescriptor);

    contextOptions.Apply(services);

    return services;
  }

  /// <summary>
  /// Registers IKSqlDBContext and its dependencies as services in the <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">The IServiceCollection to add services to.</param>
  /// <param name="ksqlDbUrl">ksqlDb connection.</param>
  /// <param name="setupAction">Action to configure the KSqlDbContextOptions for the context.</param>
  /// <param name="contextLifetime">The lifetime with which to register the context service in the container.</param>
  /// <param name="restApiLifetime">The lifetime with which to register the IKSqlDbRestApiClient service in the container.</param>
  /// <returns>The original IServiceCollection.</returns>
  public static IServiceCollection ConfigureKSqlDb(this IServiceCollection services,
    string ksqlDbUrl,
    Action<ISetupParameters> setupAction = null,
    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
    ServiceLifetime restApiLifetime = ServiceLifetime.Scoped)
  {
    return services.ConfigureKSqlDb<IKSqlDBContext, KSqlDBContext>(ksqlDbUrl, setupAction, contextLifetime, restApiLifetime);
  }

  /// <summary>
  /// Registers the given ksqldb context and its dependencies as services in the <see cref="IServiceCollection" />.
  /// </summary>
  /// <typeparam name="TContextService">The type of context to be registered as.</typeparam>
  /// <typeparam name="TContextImplementation">The implementation type of the context to be registered.</typeparam>
  /// <param name="services">The IServiceCollection to add services to.</param>
  /// <param name="ksqlDbUrl">ksqlDb connection.</param>
  /// <param name="setupAction">Optional action to configure the KSqlDbContextOptions for the context.</param>
  /// <param name="contextLifetime">The lifetime with which to register the context service in the container.</param>
  /// <param name="restApiLifetime">The lifetime with which to register the IKSqlDbRestApiClient service in the container.</param>
  /// <returns>The original IServiceCollection.</returns>
  public static IServiceCollection ConfigureKSqlDb<TContextService, TContextImplementation>(this IServiceCollection services, 
    string ksqlDbUrl,
    Action<ISetupParameters> setupAction = null,
    ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
    ServiceLifetime restApiLifetime = ServiceLifetime.Scoped)
    where TContextService : IKSqlDBContext
    where TContextImplementation : IKSqlDBContext
  {
    var builder = new KSqlDbContextOptionsBuilder();

    var setupParameters = builder.UseKSqlDb(ksqlDbUrl);

    setupAction?.Invoke(setupParameters);

    return services.ConfigureKSqlDb<TContextService, TContextImplementation>(builder, contextLifetime, restApiLifetime);
  }
}