using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Json
{
  internal class JsonTypeInfoResolver(IJsonTypeInfoResolver typeInfoResolver) : IJsonTypeInfoResolver
  {
    internal IJsonTypeInfoResolver TypeInfoResolver => typeInfoResolver ?? throw new ArgumentNullException(nameof(typeInfoResolver));

    private IList<Action<JsonTypeInfo>>? modifiers;
    internal IList<Action<JsonTypeInfo>> Modifiers => modifiers ??= new List<Action<JsonTypeInfo>>();

    public virtual JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
      var typeInfo = TypeInfoResolver.GetTypeInfo(type, options);
      if (typeInfo == null)
        return null;

      if (modifiers == null)
        return typeInfo;

      foreach (var modifier in modifiers)
      {
        modifier(typeInfo);
      }

      return typeInfo;
    }
  }
}
