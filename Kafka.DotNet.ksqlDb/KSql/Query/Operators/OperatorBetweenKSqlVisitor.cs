using System.Linq.Expressions;
using System.Text;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Operators
{
  internal class OperatorBetweenKSqlVisitor : KSqlVisitor
  {
    public OperatorBetweenKSqlVisitor(StringBuilder stringBuilder)
      : base(stringBuilder, useTableAlias: false)
    {
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
      var methodInfo = methodCallExpression.Method;

      if (methodCallExpression.Object == null
          && methodInfo.DeclaringType.Name == nameof(KSqlOperatorExtensions))
      {
        switch (methodInfo.Name)
        {
          case nameof(KSqlOperatorExtensions.Between):
            
            Visit(methodCallExpression.Arguments[0]);

            Append(" BETWEEN ");

            Visit(methodCallExpression.Arguments[1]);

            Append(" AND ");

            Visit(methodCallExpression.Arguments[2]);
            
            break;
        }

      }
      else base.VisitMethodCall(methodCallExpression);

      return methodCallExpression;
    }
  }
}