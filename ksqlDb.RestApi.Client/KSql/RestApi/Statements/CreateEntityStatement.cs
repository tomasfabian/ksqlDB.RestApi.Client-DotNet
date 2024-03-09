using System.Reflection;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal class CreateEntityStatement
{
  private static readonly EntityProvider EntityProvider = new();

  protected static IEnumerable<MemberInfo> Members<T>(bool? includeReadOnly = null)
  {
    return Members(typeof(T), includeReadOnly);
  }

  protected static IEnumerable<MemberInfo> Members(Type type, bool? includeReadOnly = null)
  {
    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

    var properties = type
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(c => c.CanWrite || (includeReadOnly.HasValue && includeReadOnly.Value))
      .OfType<MemberInfo>()
      .Concat(fields);
      
    return properties.Where(c => !c.GetCustomAttributes().OfType<IgnoreByInsertsAttribute>().Any());
  }

  protected static string GetEntityName<T>(IEntityProperties metadata)
  {
    return EntityProvider.GetFormattedName<T>(metadata);
  }

  protected static Type GetMemberType(MemberInfo memberInfo)
  {
    var type = memberInfo.MemberType switch
    {
      MemberTypes.Field => ((FieldInfo) memberInfo).FieldType,
      MemberTypes.Property => ((PropertyInfo) memberInfo).PropertyType,
      _ => throw new ArgumentOutOfRangeException(nameof(memberInfo), $"Unknown '{nameof(MemberTypes)}' value {memberInfo.MemberType}")
    };

    return type;
  }
}
