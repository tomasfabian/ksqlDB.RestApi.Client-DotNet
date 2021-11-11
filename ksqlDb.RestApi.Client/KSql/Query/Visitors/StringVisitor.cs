using System.Linq.Expressions;
using System.Text;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal class StringVisitor : KSqlVisitor
  {
    public StringVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
      : base(stringBuilder, queryMetadata)
    {
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
      if (methodCallExpression.Object?.Type != typeof(string)) 
        return methodCallExpression;

      var methodInfo = methodCallExpression.Method;

      switch (methodInfo.Name)
      {
        case nameof(string.ToUpper):
          Append("UCASE(");
          Visit(methodCallExpression.Object);
          Append(")");
          break;
        case nameof(string.ToLower):
          Append("LCASE(");
          Visit(methodCallExpression.Object);
          Append(")");
          break;
        case nameof(string.StartsWith):
          VisitStartsWith(methodCallExpression);

          break;
      }

      return methodCallExpression;
    }

    private void VisitStartsWith(MethodCallExpression methodCallExpression)
    {
      Visit(methodCallExpression.Object);

      Append(" LIKE ");

      Visit(methodCallExpression.Arguments[0]);

      StringBuilder.Replace("'", "%'", StringBuilder.Length - 1, 1);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      Append(node.Name);

      return base.VisitParameter(node);
    }
  }
}