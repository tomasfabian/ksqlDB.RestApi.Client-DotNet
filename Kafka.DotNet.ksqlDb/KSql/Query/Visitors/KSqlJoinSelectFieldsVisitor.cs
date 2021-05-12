using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal sealed class KSqlJoinSelectFieldsVisitor : KSqlVisitor
  {
    internal KSqlJoinSelectFieldsVisitor(StringBuilder stringBuilder)
      : base(stringBuilder, useTableAlias: true)
    {
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
        Append(((ParameterExpression)memberExpression.Expression).Name);
        Append(".");
        Append(memberExpression.Member.Name);
      }
      else
        base.VisitMember(memberExpression);

      return memberExpression;
    }
  }
}