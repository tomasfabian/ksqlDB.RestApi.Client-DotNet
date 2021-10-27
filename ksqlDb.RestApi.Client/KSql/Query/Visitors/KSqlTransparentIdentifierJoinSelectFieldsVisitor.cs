using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDb.RestApi.Client.KSql.Entities;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal sealed class KSqlTransparentIdentifierJoinSelectFieldsVisitor : KSqlVisitor
  {
    private readonly FromItem[] fromItems;

    internal KSqlTransparentIdentifierJoinSelectFieldsVisitor(StringBuilder stringBuilder, FromItem[] fromItems)
      : base(stringBuilder, useTableAlias: true)
    {
      this.fromItems = fromItems;
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

        var fromItem2 = fromItems.FirstOrDefault(c => c.Type == memberExpression.Expression.Type);

        if (fromItem2 != null)
          fromItem2.Alias = alias;

        Append(alias);
        Append(".");
        Append(memberExpression.Member.Name);

        return memberExpression;
      }

      var fromItem = fromItems.FirstOrDefault(c => c.Type == memberExpression.Member.DeclaringType);

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
}