using System.Reflection;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal class EntityInfo(ModelBuilder modelBuilder)
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
      .OfType<MemberInfo>()
      .Concat(fields);

    var entityMetadata = modelBuilder.GetEntities().FirstOrDefault(c => c.Type == type);
    
    return properties.Where(c =>
    {
      var fieldMetadata = entityMetadata?.GetFieldMetadataBy(c);
      if (fieldMetadata is {Ignore: true}) return false;

      return !c.GetCustomAttributes().OfType<IgnoreByInsertsAttribute>().Any();
    });
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
