using System.Reflection;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers;
using ksqlDb.RestApi.Client.Metadata;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal class EntityInfo(IMetadataProvider metadataProvider)
{
  protected static readonly EntityProvider EntityProvider = new();

  protected IEnumerable<MemberInfo> Members<T>(bool? includeReadOnly = null)
  {
    return Members(typeof(T), includeReadOnly);
  }

  protected IEnumerable<MemberInfo> Members(Type type, bool? includeReadOnly = null)
  {
    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

    var properties = type
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(c => c.CanWrite || (includeReadOnly.HasValue && includeReadOnly.Value))
      .OfType<MemberInfo>();

    var members = properties.Concat(fields);

    var entityMetadata = metadataProvider.GetEntities().FirstOrDefault(c => c.Type == type)
                         ?? metadataProvider.GetEntities().FirstOrDefault(c => c.FieldsMetadata.Any(fm => fm.MemberInfo.DeclaringType == type));

    return members.Where(memberInfo => IncludeMemberInfo(type, entityMetadata, memberInfo, includeReadOnly));
  }

  protected virtual bool IncludeMemberInfo(Type type, EntityMetadata? entityMetadata, MemberInfo memberInfo, bool? includeReadOnly = null)
  {
    var fieldMetadata = entityMetadata?.GetFieldMetadataBy(memberInfo);

    var subType = GetMemberType(memberInfo);
    if (!type.IsGenericType && IsStructType(subType, memberInfo))
    {
      var subMembers = Members(subType, includeReadOnly);

      if(!subMembers.Any())
        return false;
    }

    return fieldMetadata is not {Ignore: true} && !memberInfo.GetCustomAttributes().OfType<IgnoreAttribute>().Any();
  }

  protected bool IsStructType(Type type, MemberInfo? memberInfo)
  {
    if (type.TryGetAttribute<StructAttribute>() != null)
      return true;

    if (memberInfo == null)
      return false;

    var entityMetadata =
      metadataProvider.GetEntities().FirstOrDefault(c => c.Type == type)
      ?? metadataProvider
        .GetEntities()
        .FirstOrDefault(c => c.FieldsMetadata.Any(fm => fm.MemberInfo.DeclaringType == type));
    var fieldMetadata = entityMetadata?.GetFieldMetadataBy(memberInfo);
    return fieldMetadata is
    {
      IsStruct: true
    };
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
