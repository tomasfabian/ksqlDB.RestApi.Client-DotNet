using System.Reflection;

namespace ksqlDb.RestApi.Client.Metadata
{
  internal class EntityMetadata
  {
    public Type Type { get; internal set; } = null!;
    internal MemberInfo? PrimaryKeyMemberInfo { get; set; }

    internal readonly IDictionary<MemberInfo, FieldMetadata> FieldsMetadataDict = new Dictionary<MemberInfo, FieldMetadata>();
    public IEnumerable<FieldMetadata> FieldsMetadata => FieldsMetadataDict.Values;
  }
}
