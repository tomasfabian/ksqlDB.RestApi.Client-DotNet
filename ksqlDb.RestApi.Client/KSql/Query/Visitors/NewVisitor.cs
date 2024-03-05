using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal class NewVisitor : KSqlFunctionVisitor
  {
    internal NewVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
      : base(stringBuilder, queryMetadata)
    {
    }

    protected override Expression VisitNew(NewExpression newExpression)
    {
      if (newExpression == null) throw new ArgumentNullException(nameof(newExpression));

      if (newExpression.Members != null && newExpression.Type.IsAnonymousType())
      {
        bool isFirst = true;

        foreach (var memberWithArguments in newExpression.Members.Zip(newExpression.Arguments,
                   (x, y) => new { First = x, Second = y }))
        {
          if (isFirst)
            isFirst = false;
          else
            Append(ColumnsSeparator);

          switch (memberWithArguments.Second.NodeType)
          {
            case ExpressionType.Not:
            case ExpressionType.TypeAs:
            case ExpressionType.ArrayLength:
            case ExpressionType.Constant:
            case ExpressionType.NewArrayInit:
            case ExpressionType.ListInit:
            case ExpressionType.MemberInit:
            case ExpressionType.Call:
              Visit(memberWithArguments.Second);
              Append(" ");
              break;
            case ExpressionType.MemberAccess:
              if (memberWithArguments.Second is MemberExpression
                {
                  Expression: MemberInitExpression memberInitExpression
                } && memberInitExpression.Type.IsStruct())
              {
                Visit(memberWithArguments.Second);
                Append(" ");
              }
              else if (memberWithArguments.Second.NodeType == ExpressionType.MemberAccess &&
                       memberWithArguments.Second is MemberExpression me5 && me5.Expression?.Type != null &&
                       me5.Expression.Type.IsKsqlGrouping())
              {
                VisitMemberWithArguments(memberWithArguments.First, memberWithArguments.Second);

                continue;
              }

              if (memberWithArguments.Second is MemberExpression { Expression: null } && TryVisitTimeTypes(memberWithArguments.Second))
              {
                Append(" ");
              }

              break;

            case ExpressionType.Conditional:
              Append("CASE");
              Visit(memberWithArguments.Second);
              Append(" END AS ");
              break;
          }

          if (memberWithArguments.Second is BinaryExpression)
          {
            PrintColumnWithAlias(memberWithArguments.First, memberWithArguments.Second);

            continue;
          }

          VisitMemberWithArguments(memberWithArguments.First, memberWithArguments.Second);
        }
      }
      else
        TryVisitTimeTypes(newExpression);

      return newExpression;
    }

    private protected void VisitMemberWithArguments(MemberInfo memberInfo, Expression expression)
    {
      if (ShouldAppendAlias(memberInfo, expression))
        PrintColumnWithAlias(memberInfo, expression);
      else
        ProcessVisitNewMember(memberInfo, expression);
    }
  }
}
