using System.Reflection;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Pluralize.NET;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal class CreateEntityStatement
{
  protected static readonly IPluralize EnglishPluralizationService = new Pluralizer();

  protected IEnumerable<MemberInfo> Members<T>(bool? includeReadOnly = null)
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

  protected static string GetEntityName<T>(IEntityCreationProperties metadata)
  {
    string entityName = metadata?.EntityName;

    if (string.IsNullOrEmpty(entityName))
      entityName = typeof(T).Name;

    if (metadata is { ShouldPluralizeEntityName: true })
      entityName = EnglishPluralizationService.Pluralize(entityName);

    return IdentifierUtil.Format(entityName, metadata?.IdentifierEscaping ?? IdentifierEscaping.Never);
  }

  protected static Type GetMemberType(MemberInfo memberInfo)
  {
    var type = memberInfo.MemberType switch
    {
      MemberTypes.Field => ((FieldInfo) memberInfo).FieldType,
      MemberTypes.Property => ((PropertyInfo) memberInfo).PropertyType,
      _ => throw new ArgumentOutOfRangeException(nameof(memberInfo.MemberType))
    };

    return type;
  }
}
