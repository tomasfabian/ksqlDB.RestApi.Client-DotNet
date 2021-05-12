using System.Linq;

namespace Kafka.DotNet.ksqlDB.Infrastructure.Extensions
{
  internal static class EnumerableExtensions
  {
    public static bool IsOneOfFollowing<TItem>(this TItem item, params TItem[] allowedValues)
    {
      return allowedValues.Any(c => c.Equals(item));
    }

    public static bool IsNotOneOfFollowing<TItem>(this TItem item, params TItem[] allowedValues)
    {
      return !item.IsOneOfFollowing(allowedValues);
    }
  }
}