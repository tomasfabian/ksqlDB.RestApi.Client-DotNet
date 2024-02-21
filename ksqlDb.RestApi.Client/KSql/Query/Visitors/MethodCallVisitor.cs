using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Operators;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class MethodCallVisitor : KSqlVisitor
{
  private readonly StringBuilder stringBuilder;
  private readonly KSqlQueryMetadata queryMetadata;

  public MethodCallVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
    this.stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
    this.queryMetadata = queryMetadata ?? throw new ArgumentNullException(nameof(queryMetadata));
  }

  protected override KSqlFunctionVisitor CreateKSqlFunctionVisitor()
  {
    return new KSqlFunctionLambdaVisitor(stringBuilder, queryMetadata);
  }

  public override Expression Visit(Expression expression)
  {
    if (expression == null)
      return null;

    switch (expression.NodeType)
    {
      case ExpressionType.Call:
        VisitMethodCall((MethodCallExpression)expression);
        break;

      default:
        base.Visit(expression);
        break;
    }

    return expression;
  }

  protected override Expression VisitParameter(ParameterExpression node)
  {
    if(queryMetadata.IsInNestedFunctionScope)
      Append(node.Name);
  
    return node;
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    var methodInfo = methodCallExpression.Method;

    if (methodCallExpression.Object != null && methodCallExpression.Object.Type.IsDictionary())
    {
      if (methodCallExpression.Method.Name == "get_Item")
      {
        Visit(methodCallExpression.Object);
        Append("[");
        Visit(methodCallExpression.Arguments[0]);
        Append("]");
      }
    }

    TryCast(methodCallExpression);

    if (methodCallExpression.Object == null && methodInfo.DeclaringType is { Name: nameof(KSqlInvocationFunctionsExtensions) })
    {
      new KSqlInvocationFunctionVisitor(stringBuilder, queryMetadata).Visit(methodCallExpression);

      return methodCallExpression;
    }

    if (methodCallExpression.Object == null && methodInfo.DeclaringType is { Name: nameof(KSqlFunctionsExtensions) })
    {
      CreateKSqlFunctionVisitor().Visit(methodCallExpression);

      return methodCallExpression;
    }

    new KSqlCustomFunctionVisitor(stringBuilder, queryMetadata).Visit(methodCallExpression);

    if (methodCallExpression.Object != null && (methodInfo.DeclaringType != null && methodInfo.DeclaringType.Name == typeof(IAggregations<>).Name || methodInfo.DeclaringType is
        {
          Name: nameof(IAggregations)
        }))
    {
      new AggregationFunctionVisitor(stringBuilder, queryMetadata).Visit(methodCallExpression);
    }

    if (methodCallExpression.Object == null && methodInfo.DeclaringType is { Name: nameof(KSqlOperatorExtensions) })
    {
      new OperatorBetweenKSqlVisitor(stringBuilder, queryMetadata).Visit(methodCallExpression);
    }

    if (methodCallExpression.Object?.Type == typeof(string))
    {
      new StringVisitor(stringBuilder, queryMetadata).Visit(methodCallExpression);
    }

    TryPrintEquals(methodCallExpression, methodInfo);

    TryPrintContains(methodCallExpression, methodInfo);

    return methodCallExpression;
  }

  private void TryPrintEquals(MethodCallExpression methodCallExpression, MethodInfo methodInfo)
  {
    if (methodInfo.Name == "Equals" && methodCallExpression.Arguments.Count == 1 &&
        methodInfo.ReturnType == typeof(bool))
    {
      Visit(methodCallExpression.Object);
      Append($" {BinaryOperators.Equal} ");
      Visit(methodCallExpression.Arguments[0]);
    }
  }

  private protected void TryCast(MethodCallExpression methodCallExpression)
  {
    var methodName = methodCallExpression.Method.Name;

    if (methodName.IsOneOfFollowing(nameof(string.ToString), nameof(Convert.ToInt32), nameof(Convert.ToInt64), nameof(Convert.ToDecimal), nameof(Convert.ToDouble)))
    {
      Append("CAST(");

      Visit(methodCallExpression.Arguments.Count >= 1
        ? methodCallExpression.Arguments[0]
        : methodCallExpression.Object);

      string ksqlType = methodName switch
      {
        nameof(string.ToString) => KSqlTypes.Varchar,
        nameof(Convert.ToInt32) => KSqlTypes.Int,
        nameof(Convert.ToInt64) => KSqlTypes.BigInt,
        nameof(KSQLConvert.ToDecimal) => $"{KSqlTypes.Decimal}({methodCallExpression.Arguments[1]},{methodCallExpression.Arguments[2]})",
        nameof(Convert.ToDouble) => KSqlTypes.Double,
        _ => throw new ArgumentOutOfRangeException(nameof(methodName))
      };

      Append($" AS {ksqlType})");
    }
  }
}
