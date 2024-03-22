using System.Reflection;

namespace ksqlDb.RestApi.Client.KSql.Metadata;

internal class FieldMetadata
{
  internal MemberInfo MemberInfo { get; init; } = null!;
}
