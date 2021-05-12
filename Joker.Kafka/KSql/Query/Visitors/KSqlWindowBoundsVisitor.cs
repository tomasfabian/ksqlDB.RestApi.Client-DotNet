using System;
using System.Linq.Expressions;
using System.Text;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal class KSqlWindowBoundsVisitor : KSqlVisitor
  {
    public KSqlWindowBoundsVisitor(StringBuilder stringBuilder)
      : base(stringBuilder, useTableAlias: false)
    {
    }

    protected override Expression VisitMember(MemberExpression memberExpression)
    {
      if (memberExpression.Type == typeof(Bounds))
      {
        Append(memberExpression.Member.Name.ToUpper());
      }

      return memberExpression;
    }
  }
}