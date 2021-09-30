using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal class KSqlInvocationFunctionVisitor : KSqlVisitor
  {
    private readonly StringBuilder stringBuilder;

    public KSqlInvocationFunctionVisitor(StringBuilder stringBuilder)
      : base(stringBuilder, useTableAlias: false)
    {
      this.stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
    }

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
            
            VisitArgument(methodCallExpression.Arguments[1]);

            Append(", ");
            
            if (methodCallExpression.Arguments.Count >= 3)
              VisitArgument(methodCallExpression.Arguments[2]);

            if (methodCallExpression.Arguments.Count == 4)
            {
              Append(", ");

              VisitArgument(methodCallExpression.Arguments[3]);
            }

            Append(")");

            break;
        }
      }
      else base.VisitMethodCall(methodCallExpression);

      return methodCallExpression;
    }

    private void VisitArgument(Expression expression)
    {
      new LambdaVisitor(stringBuilder).Visit(expression);
    }
  }
}