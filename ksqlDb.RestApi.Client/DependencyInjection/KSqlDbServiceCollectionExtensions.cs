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
    public static void ConfigureKSqlDb(this IServiceCollection services, 
      string ksqlDbUrl,
      Action<ISetupParameters> setupAction = null,
      ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
    {
      var builder = new KSqlDbContextOptionsBuilder();

      var setupParameters = builder.UseKSqlDb(ksqlDbUrl);

      setupAction?.Invoke(setupParameters);

      var contextOptions = setupParameters.Options;

      services.AddSingleton(contextOptions);

      var contextDescriptor = new ServiceDescriptor(
        typeof(IKSqlDBContext),
        typeof(KSqlDBContext),
        contextLifetime);

      services.Add(contextDescriptor);

      var uri = new Uri(contextOptions.Url);

      services.AddSingleton<IHttpClientFactory, HttpClientFactory>(_ => new HttpClientFactory(uri));
      services.AddScoped<IKSqlDbRestApiClient, KSqlDbRestApiClient>();
    }
  }
}