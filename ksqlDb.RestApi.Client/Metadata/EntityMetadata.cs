using System.Reflection;

namespace ksqlDb.RestApi.Client.Metadata
{
  internal sealed class EntityMetadata
  {
    public Type Type { get; internal set; } = null!;

    internal MemberInfo? PrimaryKeyMemberInfo { get; set; }

    internal readonly IDictionary<MemberInfo, FieldMetadata> FieldsMetadataDict = new Dictionary<MemberInfo, FieldMetadata>();

    public IEnumerable<FieldMetadata> FieldsMetadata => FieldsMetadataDict.Values;

    internal bool Add(MemberInfo memberInfo)
    {
      if (!FieldsMetadataDict.ContainsKey(memberInfo))
      {
        var fieldMetadata = new FieldMetadata
        {
          MemberInfo = memberInfo
        };
        FieldsMetadataDict[memberInfo] = fieldMetadata;
        return true;
      }

      return false;
    }

    public FieldMetadata? GetFieldMetadataBy(MemberInfo memberInfo)
    {
      return FieldsMetadataDict.Values.FirstOrDefault(c =>
        c.MemberInfo.DeclaringType == memberInfo.DeclaringType && c.MemberInfo.Name == memberInfo.Name);
    }

    internal MemberInfo? TryGetMemberInfo(string memberInfoName)
    {
      return FieldsMetadata.Where(c => c.MemberInfo.Name == memberInfoName).Select(c => c.MemberInfo).FirstOrDefault();
    }
  }
}
