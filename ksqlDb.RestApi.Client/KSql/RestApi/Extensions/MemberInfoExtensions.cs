using System.Reflection;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Extensions
{
  internal static class MemberInfoExtensions
  {
    /// <summary>
    /// Format the <c>identifier</c>.
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <param name="escaping"></param>
    /// <returns>the <c>memberInfo.Name</c> modified based on the provided <c>format</c></returns>
    public static string Format(this MemberInfo memberInfo, IdentifierEscaping escaping) => IdentifierUtil.Format(memberInfo.Name, escaping);
  }
}
