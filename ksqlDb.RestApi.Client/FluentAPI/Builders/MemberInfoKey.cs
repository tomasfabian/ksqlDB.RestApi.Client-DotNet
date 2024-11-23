using System.Reflection;

namespace ksqlDb.RestApi.Client.FluentAPI.Builders
{
  internal record MemberInfoKey
  {
    internal Module Module { get; set; }
    internal int MetadataToken { get; set; }
  }
}
