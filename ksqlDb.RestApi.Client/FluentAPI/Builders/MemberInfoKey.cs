using System.Reflection;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders;

internal record MemberInfoKey
{
  internal Module Module { get; set; }
  internal int MetadataToken { get; set; }
}

internal static class MemberInfoExtensions
{
  internal static MemberInfoKey ToMemberInfoKey(this MemberInfo memberInfo)
  {
    return new MemberInfoKey
    {
      Module = memberInfo.Module,
      MetadataToken = memberInfo.MetadataToken
    };
  }
}
