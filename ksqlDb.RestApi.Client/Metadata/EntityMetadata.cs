using System.Linq.Expressions;
using System.Reflection;

namespace ksqlDb.RestApi.Client.Metadata
{
  internal sealed class EntityMetadata
  {
    public Type Type { get; internal set; } = null!;

    internal MemberInfo? PrimaryKeyMemberInfo { get; set; }

    internal readonly IDictionary<MemberInfo, FieldMetadata> FieldsMetadataDict = new Dictionary<MemberInfo, FieldMetadata>();

    public IEnumerable<FieldMetadata> FieldsMetadata => FieldsMetadataDict.Values;


    private readonly IList<MemberExpression> fieldMemberExpressions = new List<MemberExpression>();

    internal bool Add(MemberExpression memberExpression)
    {
      if (fieldMemberExpressions.All(c => c.Type != memberExpression.Type && c.Member != memberExpression.Member))
      {
        fieldMemberExpressions.Add(memberExpression);
        return true;
      }

      return false;
    }

    public FieldMetadata? GetFieldMetadataBy(MemberInfo memberInfo)
    {
      return FieldsMetadataDict.Values.FirstOrDefault(c =>
        c.MemberInfo.DeclaringType == memberInfo.DeclaringType && c.MemberInfo.Name == memberInfo.Name);
    }

    internal MemberExpression? TryGetMemberExpression(string memberInfoName)
    {
      return fieldMemberExpressions.Where(c => c.Member.Name == memberInfoName).Select(c => c).FirstOrDefault();
    }
  }
}
