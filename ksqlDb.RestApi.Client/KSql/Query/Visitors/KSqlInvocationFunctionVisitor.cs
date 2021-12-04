using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal class KSqlInvocationFunctionVisitor : KSqlVisitor
  {
    private readonly StringBuilder stringBuilder;
    private readonly KSqlQueryMetadata queryMetadata;

    public KSqlInvocationFunctionVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
      : base(stringBuilder, queryMetadata)
    {
      this.stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
      this.queryMetadata = queryMetadata ?? throw new ArgumentNullException(nameof(queryMetadata));
    }

    private static bool isNestedInvocationFunction;

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
      var methodInfo = methodCallExpression.Method;

      if (methodCallExpression.Object == null
          && methodInfo.DeclaringType.Name == nameof(KSqlInvocationFunctionsExtensions))
      {
        switch (methodInfo.Name)
        {
          case nameof(KSqlInvocationFunctionsExtensions.Transform):
          case nameof(KSqlInvocationFunctionsExtensions.Filter):
          case nameof(KSqlInvocationFunctionsExtensions.Reduce):

            Append($"{methodInfo.Name.ToKSqlFunctionName()}(");

            var arguments = methodCallExpression.Arguments.ToList();

            if (arguments[0].Type == typeof(KSqlFunctions))
              arguments = arguments.Skip(1).ToList();

            if (isNestedInvocationFunction)
              VisitArgument(arguments[0]);
            else
              base.Visit(arguments[0]);

            isNestedInvocationFunction = true;

            Append(", ");
            
            if (arguments.Count >= 2)
              VisitArgument(arguments[1]);

            if (arguments.Count == 3)
            {
              Append(", ");

              VisitArgument(arguments[2]);
            }

            Append(")");

            break;
        }
      }
      else base.VisitMethodCall(methodCallExpression);
      
      isNestedInvocationFunction = false;

      return methodCallExpression;
    }

    private void VisitArgument(Expression expression)
    {
      new LambdaVisitor(stringBuilder, queryMetadata).Visit(expression);
    }
  }
}