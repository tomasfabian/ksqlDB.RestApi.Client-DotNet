using System.Reflection;

namespace ksqlDb.RestApi.Client.Metadata
{
  internal sealed class EntityMetadata
  {
    public Type Type { get; internal set; } = null!;

    internal MemberInfo? PrimaryKeyMemberInfo { get; set; }

    internal readonly IDictionary<MemberInfo, FieldMetadata> FieldsMetadataDict = new Dictionary<MemberInfo, FieldMetadata>();

    public IEnumerable<FieldMetadata> FieldsMetadata => FieldsMetadataDict.Values;

    public FieldMetadata? GetFieldMetadataBy(MemberInfo memberInfo)
    {
      return FieldsMetadataDict.Values.FirstOrDefault(c =>
        c.MemberInfo.DeclaringType == memberInfo.DeclaringType && c.MemberInfo.Name == memberInfo.Name);
    }
  }
}
