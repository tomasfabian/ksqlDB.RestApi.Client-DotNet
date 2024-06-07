using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Json
{
  internal static class JsonSerializerOptionsExtensions
  {
    public static void WithModifier(this JsonSerializerOptions jsonSerializerOptions, Action<JsonTypeInfo> modifier)
    {
      if (jsonSerializerOptions.TypeInfoResolver == null)
      {
        var defaultJsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();
        var resolver = new JsonTypeInfoResolver(defaultJsonTypeInfoResolver)
        {
          Modifiers = { modifier }
        };
        jsonSerializerOptions.TypeInfoResolver = resolver;
      }
      else if (jsonSerializerOptions.TypeInfoResolver is not JsonTypeInfoResolver)
      {
        var resolver = new JsonTypeInfoResolver(jsonSerializerOptions.TypeInfoResolver)
        {
          Modifiers = { modifier }
        };

        jsonSerializerOptions.TypeInfoResolver = resolver;
      }
    }
  }
}
