﻿using System;
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
            
            if(isNestedInvocationFunction)
              VisitArgument(methodCallExpression.Arguments[1]);
            else
              base.Visit(methodCallExpression.Arguments[1]);

            isNestedInvocationFunction = true;

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
      
      isNestedInvocationFunction = false;

      return methodCallExpression;
    }

    private void VisitArgument(Expression expression)
    {
      new LambdaVisitor(stringBuilder, queryMetadata).Visit(expression);
    }
  }
}