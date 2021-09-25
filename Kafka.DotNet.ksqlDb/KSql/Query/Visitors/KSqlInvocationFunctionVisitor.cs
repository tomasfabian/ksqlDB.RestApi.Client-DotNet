using System;
using System.Collections;
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

            Append($"{methodInfo.Name.ToKSqlFunctionName()}(");

            Visit(methodCallExpression.Arguments[1]);

            Append(", ");

            new LambdaVisitor(stringBuilder).Visit(methodCallExpression.Arguments[2]);

            Append(")");

            break;
        }
      }
      else base.VisitMethodCall(methodCallExpression);

      return methodCallExpression;
    }
  }
}