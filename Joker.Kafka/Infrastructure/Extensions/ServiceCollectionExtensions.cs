using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.DotNet.ksqlDB.Infrastructure.Extensions
{
  internal static class ServiceCollectionExtensions
  {
    internal static bool HasRegistration<TType>(this IServiceCollection serviceCollection)
    {
      return serviceCollection.Any(x => x.ServiceType == typeof(TType));
    }
  }
}