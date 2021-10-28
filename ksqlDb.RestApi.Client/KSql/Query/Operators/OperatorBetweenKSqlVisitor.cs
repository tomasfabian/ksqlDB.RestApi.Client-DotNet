using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;

namespace ksqlDB.RestApi.Client.KSql.Query.Operators
{
  internal class OperatorBetweenKSqlVisitor : KSqlVisitor
  {
    public OperatorBetweenKSqlVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
      : base(stringBuilder, queryMetadata)
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
            
            PrintBetween(methodCallExpression);

            break;

          case nameof(KSqlOperatorExtensions.NotBetween):
            
            PrintBetween(methodCallExpression, negated: true);

            break;
        }

      }
      else base.VisitMethodCall(methodCallExpression);

      return methodCallExpression;
    }

    private void PrintBetween(MethodCallExpression methodCallExpression, bool negated = false)
    {
      Visit(methodCallExpression.Arguments[0]);

      if(negated)
        Append(" NOT");

      Append(" BETWEEN ");

      Visit(methodCallExpression.Arguments[1]);

      Append(" AND ");

      Visit(methodCallExpression.Arguments[2]);
    }
  }
}