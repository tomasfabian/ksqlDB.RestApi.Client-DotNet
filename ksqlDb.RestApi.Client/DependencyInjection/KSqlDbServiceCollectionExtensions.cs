using System;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDb.RestApi.Client.DependencyInjection
{
  public static class KSqlDbServiceCollectionExtensions
  {
    /// <summary>
    /// Registers the given ksqldb context factory as a service in the <see cref="IServiceCollection" />.
    /// </summary>
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

    public static IServiceCollection ConfigureKSqlDb(this IServiceCollection services,
      string ksqlDbUrl,
      Action<ISetupParameters> setupAction = null,
      ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
    {
      return services.ConfigureKSqlDb<KSqlDBContext, IKSqlDBContext>(ksqlDbUrl, setupAction, contextLifetime);
    }

    /// <summary>
    /// Registers the given ksqldb context and its dependencies as services in the <see cref="IServiceCollection" />.
    /// </summary>
    internal static IServiceCollection ConfigureKSqlDb<TFromContext, TToContext>(this IServiceCollection services,
      KSqlDbContextOptionsBuilder builder,
      ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
      where TFromContext : IKSqlDBContext
      where TToContext : IKSqlDBContext
    {
      var contextOptions = builder.InternalOptions;

      services.AddSingleton(contextOptions);

      var contextDescriptor = new ServiceDescriptor(
        typeof(TToContext),
        typeof(TFromContext),
        contextLifetime);

      services.Add(contextDescriptor);

      var uri = new Uri(contextOptions.Url);

      services.AddSingleton<IHttpClientFactory, HttpClientFactory>(_ => new HttpClientFactory(uri));
      services.AddScoped<IKSqlDbRestApiClient, KSqlDbRestApiClient>();

      return services;
    }

    /// <summary>
    /// Registers the given ksqldb context and its dependencies as services in the <see cref="IServiceCollection" />.
    /// </summary>
    public static IServiceCollection ConfigureKSqlDb<TFromContext, TToContext>(this IServiceCollection services, 
      string ksqlDbUrl,
      Action<ISetupParameters> setupAction = null,
      ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
      where TFromContext : IKSqlDBContext
      where TToContext : IKSqlDBContext
    {
      var builder = new KSqlDbContextOptionsBuilder();

      var setupParameters = builder.UseKSqlDb(ksqlDbUrl);

      setupAction?.Invoke(setupParameters);

      return services.ConfigureKSqlDb<TFromContext, TToContext>(builder, contextLifetime);
    }
  }
}