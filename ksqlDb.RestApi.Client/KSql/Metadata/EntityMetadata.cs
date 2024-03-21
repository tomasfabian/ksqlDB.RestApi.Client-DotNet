using System.Reflection;

namespace ksqlDb.RestApi.Client.KSql.Metadata;

internal class EntityMetadata
{
  internal Type Type { get; set; } = null!;

  private readonly IDictionary<MemberInfo, FieldMetadata> fieldsMetadata = new Dictionary<MemberInfo, FieldMetadata>();
  internal IEnumerable<FieldMetadata> FieldsMetadata => fieldsMetadata.Values;

  internal bool Add(MemberInfo memberInfo)
  {
    if (!fieldsMetadata.ContainsKey(memberInfo))
    {
      var fieldMetadata = new FieldMetadata
      {
        MemberInfo = memberInfo
      };
      fieldsMetadata[memberInfo] = fieldMetadata;
      return true;
    }

    return false;
  }

  internal MemberInfo? TryGetMemberInfo(string memberInfoName)
  {
    return FieldsMetadata.Where(c => c.MemberInfo.Name == memberInfoName).Select(c => c.MemberInfo).FirstOrDefault();
  }
}
