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
      if (methodCallExpression.Type != typeof(string)) 
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
      }

      return methodCallExpression;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      Append(node.Name);

      return base.VisitParameter(node);
    }
  }
}