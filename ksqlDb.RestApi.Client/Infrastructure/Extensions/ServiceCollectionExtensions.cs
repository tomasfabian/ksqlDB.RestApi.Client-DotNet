using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Parameters;
using ksqlDB.RestApi.Client.KSql.RestApi.Query;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ksqlDB.RestApi.Client.Infrastructure.Extensions;

internal static class ServiceCollectionExtensions
{
  internal static bool HasRegistration<TType>(this IServiceCollection serviceCollection)
  {
    return serviceCollection.Any(x => x.ServiceType == typeof(TType));
  }

  internal static ServiceDescriptor? TryGetRegistration<TType>(this IServiceCollection serviceCollection)
  {
    return serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(TType));
  }

  internal static IServiceCollection ConfigureHttpClients(this IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    var uri = new Uri(contextOptions.Url);

    if (!serviceCollection.HasRegistration<IHttpV1ClientFactory>())
    {
      var httpClientV1Builder = serviceCollection.AddHttpClient<IHttpV1ClientFactory, HttpClientFactory>(httpClient =>
      {
        httpClient.BaseAddress = uri;
      });

      if (contextOptions.UseBasicAuth)
      {
        httpClientV1Builder.AddHttpMessageHandler(_ => new BasicAuthHandler(contextOptions));
      }
    }

    if (!serviceCollection.HasRegistration<IHttpClientFactory>())
    {
      var httpClientBuilder = serviceCollection.AddHttpClient<IHttpClientFactory, HttpClientFactory>(httpClient =>
      {
        httpClient.BaseAddress = uri;
#if !NETSTANDARD
        httpClient.DefaultRequestVersion = new Version(2, 0);
#endif
      });

      if (contextOptions.UseBasicAuth)
      {
        httpClientBuilder.AddHttpMessageHandler(_ => new BasicAuthHandler(contextOptions));
      }
    }

    return serviceCollection;
  }

  internal static void RegisterKSqlDbContextDependencies(this IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    serviceCollection.TryAddSingleton<KSqlDbProviderOptions>(contextOptions);
    serviceCollection.TryAddSingleton(contextOptions);

    serviceCollection.TryAddScoped<IKSqlQbservableProvider, QbservableProvider>();
    serviceCollection.TryAddScoped<ICreateStatementProvider, CreateStatementProvider>();

    serviceCollection.TryAddTransient<IKSqlQueryGenerator, KSqlQueryGenerator>();

    serviceCollection.ConfigureHttpClients(contextOptions);

    serviceCollection.TryAddSingleton(contextOptions);
    serviceCollection.TryAddScoped<IKPullSetDependencies, KPullSetDependencies>();
    serviceCollection.TryAddScoped<IKStreamSetDependencies, KStreamSetDependencies>();
    serviceCollection.TryAddScoped<IKSqlDbRestApiClient, KSqlDbRestApiClient>();
  }

  internal static void RegisterEndpointDependencies(this IServiceCollection serviceCollection, KSqlDBContextOptions contextOptions)
  {
    switch (contextOptions.EndpointType)
    {
      case EndpointType.Query:
        serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryProvider>();

        var queryParameters = new QueryParameters();
        queryParameters.FillPushQueryParametersFrom(contextOptions.QueryStreamParameters);
        serviceCollection.TryAddSingleton<IKSqlDbParameters>(queryParameters);

        var pullQueryParameters = new PullQueryParameters();
        pullQueryParameters.FillFrom(contextOptions.PullQueryParameters);
        serviceCollection.TryAddSingleton<IPullQueryParameters>(pullQueryParameters);
        break;
#if !NETSTANDARD
      case EndpointType.QueryStream:
        serviceCollection.TryAddScoped<IKSqlDbProvider, KSqlDbQueryStreamProvider>();
        serviceCollection.TryAddSingleton<IKSqlDbParameters>(contextOptions.QueryStreamParameters);

        var queryStreamParameters = new PullQueryStreamParameters();
        queryStreamParameters.FillFrom(contextOptions.PullQueryParameters);
        serviceCollection.TryAddSingleton<IPullQueryParameters>(queryStreamParameters);
        break;
#endif
      default:
        throw new ArgumentOutOfRangeException(nameof(contextOptions.EndpointType), contextOptions.EndpointType, "Non-exhaustive match");
    }
  }

  internal static void ApplyTo(this IServiceCollection externalServicesCollection, IServiceCollection servicesCollection)
  {
    foreach (var service in externalServicesCollection)
    {
      servicesCollection.Add(service);
    }
  }
}
