using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal sealed class KSqlJoinSelectFieldsVisitor : KSqlVisitor
{
  private readonly KSqlQueryMetadata queryMetadata;

  internal KSqlJoinSelectFieldsVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
    this.queryMetadata = queryMetadata;
  }

  protected override void ProcessVisitNewMember(MemberInfo memberInfo, Expression expression)
  {
    if (expression.NodeType == ExpressionType.MemberAccess)
    {
      Visit(expression);

      Append(" " + memberInfo.Name);
    }
    else
    {
      base.ProcessVisitNewMember(memberInfo, expression);
    }
  }

  protected override Expression VisitMember(MemberExpression memberExpression)
  {
    if (memberExpression == null) throw new ArgumentNullException(nameof(memberExpression));

    if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
    {
      var foundFromItem = queryMetadata.TrySetAlias(memberExpression, (_, alias) => string.IsNullOrEmpty(alias));

      var memberName = memberExpression.Member.GetMemberName();

      string alias = ((ParameterExpression)memberExpression.Expression).Name;

      Append(foundFromItem?.Alias ?? alias);
      Append(".");
      Append(memberName);

      return memberExpression;
    }

    var fromItem = queryMetadata.Joins.FirstOrDefault(c => c.Type == memberExpression.Member.DeclaringType);

    if (fromItem != null && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
    {
      string alias = ((MemberExpression)memberExpression.Expression).Member.Name;

      fromItem.Alias = alias;

      Append(alias);
        
      Append(".");

      var memberName = memberExpression.Member.GetMemberName();
      Append(memberName);
    }
    else
      base.VisitMember(memberExpression);

    return memberExpression;
  }

  private string SetAlias(MemberExpression memberExpression)
  {
    string alias = ((ParameterExpression) memberExpression.Expression).Name;

    var joinsOfType = queryMetadata.Joins.Where(c => c.Type == memberExpression.Expression.Type).ToArray();

    var fromItem2 = joinsOfType.FirstOrDefault();

    if (joinsOfType.Length > 1)
      fromItem2 = joinsOfType.FirstOrDefault(c => string.IsNullOrEmpty(c.Alias));

    if (fromItem2 != null)
      fromItem2.Alias = alias;
    return alias;
  }
}