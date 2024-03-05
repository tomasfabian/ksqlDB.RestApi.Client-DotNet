using System.Linq.Expressions;
using System.Text;
using ksqlDb.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class KSqlCustomFunctionVisitor : KSqlVisitor
{
  public KSqlCustomFunctionVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    var methodInfo = methodCallExpression.Method;

    var functionAttribute = methodInfo.TryGetAttribute<KSqlFunctionAttribute>();

    if (functionAttribute != null)
    {
      string functionName = string.IsNullOrEmpty(functionAttribute.FunctionName)
        ? methodInfo.Name.ToKSqlFunctionName()
        : functionAttribute.FunctionName;

      if (string.IsNullOrEmpty(functionName))
      {
        functionName = methodInfo.Name;
      }

      Append($"{functionName.ToUpper()}");

      PrintFunctionArguments(methodCallExpression.Arguments);
    }

    return methodCallExpression;
  }

  protected void VisitParams(NewArrayExpression node)
  {
    Append('(');

    PrintCommaSeparated(node.Expressions);

    Append(')');
  }
}
