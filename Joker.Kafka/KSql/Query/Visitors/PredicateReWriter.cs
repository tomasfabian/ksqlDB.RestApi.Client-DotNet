using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal sealed class PredicateReWriter
  {
    public static LambdaExpression Rewrite(LambdaExpression exp, string newParamName1, string newParamName2)
    {
      if (newParamName1 == null) throw new ArgumentNullException(nameof(newParamName1));
      if (newParamName2 == null) throw new ArgumentNullException(nameof(newParamName2));

      var newExpression = new PredicateReWriterVisitor(exp.Parameters, newParamName1, newParamName2).Visit(exp);

      return (LambdaExpression) newExpression;
    }

    private class PredicateReWriterVisitor : ExpressionVisitor
    {
      private readonly ReadOnlyCollection<ParameterExpression> parameterExpressions;
      private readonly string newParamName1;
      private readonly string newParamName2;

      public PredicateReWriterVisitor(ReadOnlyCollection<ParameterExpression> parameterExpressions, string newParamName1, string newParamName2)
      {
        this.parameterExpressions = parameterExpressions ?? throw new ArgumentNullException(nameof(parameterExpressions));
        this.newParamName1 = newParamName1 ?? throw new ArgumentNullException(nameof(newParamName1));
        this.newParamName2 = newParamName2 ?? throw new ArgumentNullException(nameof(newParamName2));
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        if (node == parameterExpressions[0])
        {
          var param1 = Expression.Parameter(parameterExpressions[0].Type, newParamName1);

          return param1;
        }
       
        return Expression.Parameter(parameterExpressions[1].Type, newParamName2);
      }
    }
  }
}