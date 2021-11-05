using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ksqlDB.RestApi.Client.Infrastructure.Extensions
{
  internal static class ServiceCollectionExtensions
  {
    internal static bool HasRegistration<TType>(this IServiceCollection serviceCollection)
    {
      return serviceCollection.Any(x => x.ServiceType == typeof(TType));
    }

    internal static ServiceDescriptor TryGetRegistration<TType>(this IServiceCollection serviceCollection)
    {
      return serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(TType));
    }
  }
}