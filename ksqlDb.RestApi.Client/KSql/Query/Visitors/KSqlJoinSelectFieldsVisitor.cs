using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
      string alias = ((ParameterExpression)memberExpression.Expression).Name;

      var fromItem2 = queryMetadata.Joins.FirstOrDefault(c => c.Type == memberExpression.Expression.Type);

      if (fromItem2 != null)
        fromItem2.Alias = alias;

      Append(alias);
      Append(".");
      Append(memberExpression.Member.Name);

      return memberExpression;
    }

    var fromItem = queryMetadata.Joins.FirstOrDefault(c => c.Type == memberExpression.Member.DeclaringType);

    if (fromItem != null && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
    {
      string alias = ((MemberExpression)memberExpression.Expression).Member.Name;

      fromItem.Alias = alias;

      Append(alias);
        
      Append(".");

      Append(memberExpression.Member.Name);
    }
    else
      base.VisitMember(memberExpression);

    return memberExpression;
  }
}