using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal sealed class KSqlTransparentIdentifierJoinSelectFieldsVisitor : KSqlVisitor
  {
    private readonly Type[] tableTypes;

    internal KSqlTransparentIdentifierJoinSelectFieldsVisitor(StringBuilder stringBuilder, Type[] tableTypes)
      : base(stringBuilder, useTableAlias: true)
    {
      this.tableTypes = tableTypes;
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

        return memberExpression;
      }

      bool isTableType = tableTypes.Contains(memberExpression.Member.DeclaringType);

      if (isTableType && memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
      {
        Append(((MemberExpression)memberExpression.Expression).Member.Name);
        
        Append(".");

        Append(memberExpression.Member.Name);
      }
      else
        base.VisitMember(memberExpression);

      return memberExpression;
    }
  }
}