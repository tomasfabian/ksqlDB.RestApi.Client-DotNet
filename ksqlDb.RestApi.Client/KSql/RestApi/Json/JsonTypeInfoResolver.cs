using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Json
{
  internal class JsonTypeInfoResolver(IJsonTypeInfoResolver typeInfoResolver) : IJsonTypeInfoResolver
  {
    private readonly IJsonTypeInfoResolver typeInfoResolver = typeInfoResolver ?? throw new ArgumentNullException(nameof(typeInfoResolver));

    public IList<Action<JsonTypeInfo>> Modifiers => modifiers ??= new List<Action<JsonTypeInfo>>();
    private IList<Action<JsonTypeInfo>>? modifiers;

    public virtual JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
      var typeInfo = typeInfoResolver.GetTypeInfo(type, options);
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
