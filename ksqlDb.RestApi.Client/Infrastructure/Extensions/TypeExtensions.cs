using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDB.RestApi.Client.Infrastructure.Extensions;

internal static class TypeExtensions
{
  internal static bool IsAnonymousType(this Type type)
    => type.Name.StartsWith("<>", StringComparison.Ordinal)
       && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Length > 0
       && type.Name.Contains("AnonymousType");

  internal static Type? TryFindProviderAncestor(this Type type)
  {
    var rootType = type;
    while (rootType != null)
    {
      if (rootType.Name == nameof(KSet))
      {
        return rootType;
      }

      rootType = rootType.BaseType;
    }

    return null;
  }
    
  internal static bool IsStruct(this Type source) 
  {
    return source.IsValueType && !source.IsEnum && !source.IsPrimitive;
  }

  internal static bool IsDictionary(this Type type)
  {
    if (!type.IsGenericType)
      return false;

    var isDictionary = type.GetGenericTypeDefinition() == typeof(IDictionary<,>) || type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

    return isDictionary;
  }

  internal static bool IsList(this Type type)
  {
    if (!type.IsGenericType)
      return false;

    var isList = type.GetGenericTypeDefinition() == typeof(IList<>) || type.GetGenericTypeDefinition() == typeof(List<>);

    return isList;
  }

  internal static bool HasKey(this MemberInfo typeInfo)
  {
    return typeInfo.GetCustomAttributes().OfType<KeyAttribute>().Any();
  }

  internal static IEnumerable<Type> GetEnumerableTypeDefinition(this Type type)
  {
    if (!type.IsGenericType && type == typeof(IEnumerable))
      return new[] { type };

    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
      return new[] { type };

    var enumerableTypes = type
      .GetInterfaces()
      .Where(t => t == typeof(IEnumerable) ||
                  (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

    return enumerableTypes;
  }

  internal static bool IsKsqlGrouping(this Type type)
  {
    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IKSqlGrouping<,>);
  }
    
  internal static string ExtractTypeName(this Type type)
  {
    var name = type.Name;

    if (type.IsGenericType)
      name = name.Remove(name.IndexOf('`'));

    return name;
  }
    
  internal static TAttribute? TryGetAttribute<TAttribute>(this MemberInfo memberInfo)
    where TAttribute : Attribute
  {
    var attribute = memberInfo.GetCustomAttributes()
      .OfType<TAttribute>()
      .FirstOrDefault();

    return attribute;
  }

  internal static string GetMemberName(this MemberExpression memberExpression, ModelBuilder? modelBuilder)
  {
    var entityMetadata = modelBuilder?.GetEntities().FirstOrDefault(c => c.Type == memberExpression.Expression?.Type);

    return memberExpression.Member.GetMemberName(entityMetadata);
  }

  internal static string GetMemberName(this MemberInfo memberInfo, ModelBuilder? modelBuilder)
  {
    var entityMetadata = modelBuilder?.GetEntities().FirstOrDefault(c => c.Type == memberInfo.DeclaringType);

    return memberInfo.GetMemberName(entityMetadata);
  }

  internal static string GetMemberName(this MemberInfo memberInfo, EntityMetadata? entityMetadata)
  {
    var fieldMetadata =
      entityMetadata?.FieldsMetadata.FirstOrDefault(c => c.MemberInfo.Name == memberInfo.Name);

    if (fieldMetadata != null && !string.IsNullOrEmpty(fieldMetadata.ColumnName))
      return fieldMetadata.ColumnName;

    var jsonPropertyNameAttribute = memberInfo.GetCustomAttribute<JsonPropertyNameAttribute>();

    var memberName = jsonPropertyNameAttribute?.Name ?? memberInfo.Name;

    return memberName;
  }
}
