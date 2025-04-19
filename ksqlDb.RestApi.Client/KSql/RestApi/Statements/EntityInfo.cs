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

  protected IEnumerable<MemberInfo> Members<TEntity>(bool? includeReadOnly = null)
  {
    return Members<TEntity>(typeof(TEntity), includeReadOnly);
  }

  protected IEnumerable<MemberInfo> Members<TEntity>(Type type, bool? includeReadOnly = null)
  {
    var targetType = type.IsArray ? type.GetElementType() : type;

    var fields = targetType!.GetFields(BindingFlags.Public | BindingFlags.Instance);

    var properties = targetType
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(c => c.CanWrite || (includeReadOnly.HasValue && includeReadOnly.Value))
      .OfType<MemberInfo>();

    var members = properties.Concat(fields);

    var entityMetadata = metadataProvider.GetEntities().FirstOrDefault(c => c.Type == typeof(TEntity));

    return members.Where(memberInfo => IncludeMemberInfo<TEntity>(type, entityMetadata, memberInfo, includeReadOnly));
  }

  protected virtual bool IncludeMemberInfo<TEntity>(Type type, EntityMetadata? entityMetadata, MemberInfo memberInfo, bool? includeReadOnly = null)
  {
    var fieldMetadata = entityMetadata?.GetFieldMetadataBy(memberInfo);

    var subType = GetMemberType(memberInfo);
    if (!type.IsGenericType && IsStructType<TEntity>(subType, memberInfo))
    {
      var subMembers = Members<TEntity>(subType, includeReadOnly);

      if(!subMembers.Any())
        return false;
    }

    return fieldMetadata is not {Ignore: true} && !memberInfo.GetCustomAttributes().OfType<IgnoreAttribute>().Any();
  }

  protected bool IsStructType<TEntity>(Type type, MemberInfo? memberInfo)
  {
    if (type.TryGetAttribute<StructAttribute>() != null)
      return true;

    if (memberInfo == null)
      return false;

    var entityMetadata = metadataProvider.GetEntities().FirstOrDefault(c => c.Type == typeof(TEntity));
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
