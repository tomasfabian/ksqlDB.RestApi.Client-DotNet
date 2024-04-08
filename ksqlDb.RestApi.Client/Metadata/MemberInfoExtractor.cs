using System.Linq.Expressions;
using System.Reflection;

namespace ksqlDb.RestApi.Client.Metadata
{
  internal static class MemberInfoExtractor
  {
    internal static IEnumerable<(string, MemberInfo)> GetMembers<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> propertyExpression)
    {
      if (propertyExpression.Body is not MemberExpression memberExpression)
        throw new ArgumentException("Expression is not a member access expression.", nameof(propertyExpression));

      yield return (memberExpression.Member.Name, memberExpression.Member);

      while (memberExpression.Expression is MemberExpression expression)
      {
        memberExpression = expression;
        yield return (memberExpression.Member.Name, memberExpression.Member);
      }
    }

    internal static MemberInfo GetMemberInfo<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> getProperty)
    {
      if (getProperty.Body is not MemberExpression memberExpression)
        throw new ArgumentException("Expression is not a member expression.");

      return memberExpression.Member;
    }
  }
}
