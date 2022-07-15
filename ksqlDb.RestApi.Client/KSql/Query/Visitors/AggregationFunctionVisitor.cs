using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Linq;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class AggregationFunctionVisitor : KSqlVisitor
{
  public AggregationFunctionVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    var methodInfo = methodCallExpression.Method;

    switch (methodInfo.Name)
    {
      case nameof(IAggregations.Count):
      case nameof(IAggregations.LongCount):
        if (methodCallExpression.Arguments.Count == 0)
        {
          Append($"{nameof(IAggregations.Count).ToUpper()}(*)");
        }
        if (methodCallExpression.Arguments.Count == 1)
        {
          Append($"{nameof(IAggregations.Count).ToUpper()}");
            
          PrintFunctionArguments(methodCallExpression.Arguments);
        }
        break;
      case nameof(IAggregations<object>.CountDistinct):
      case nameof(IAggregations<object>.LongCountDistinct):
        if (methodCallExpression.Arguments.Count == 1)
        {
          string countDistinctName = nameof(IAggregations<object>.CountDistinct);
          Append($"{countDistinctName.ToKSqlFunctionName()}");
            
          PrintFunctionArguments(methodCallExpression.Arguments);
        }
        break;
      case nameof(IAggregations<object>.Avg):
      case nameof(IAggregations<object>.Histogram):
      case nameof(IAggregations<object>.Min):
      case nameof(IAggregations<object>.Max):
      case nameof(IAggregations<object>.Sum):
        if (methodCallExpression.Arguments.Count == 1)
        {
          Append($"{methodInfo.Name.ToUpper()}(");
          Visit(methodCallExpression.Arguments[0]);
          Append(")");
        }

        break;
      case nameof(IAggregations<object>.CollectList):
      case nameof(IAggregations<object>.CollectSet):
        Append(methodInfo.Name.ToKSqlFunctionName());
        PrintFunctionArguments(methodCallExpression.Arguments);

        break;
      case nameof(IAggregations<object>.TopK):
      case nameof(IAggregations<object>.TopKDistinct):
        if (methodCallExpression.Arguments.Count == 2)
        {
          Append($"{methodInfo.Name.ToUpper()}");
          PrintFunctionArguments(methodCallExpression.Arguments);
        }

        break;
      case nameof(IAggregations<object>.EarliestByOffset):
      case nameof(IAggregations<object>.LatestByOffset):
      case nameof(IAggregations<object>.EarliestByOffsetAllowNulls):
      case nameof(IAggregations<object>.LatestByOffsetAllowNulls):
        if (methodCallExpression.Arguments.Count.IsOneOfFollowing(1, 2))
        {
          var functionName = GetFunctionName(methodInfo.Name);
          bool ignoreNulls = !methodInfo.Name.ToLower().EndsWith("Nulls".ToLower());
          Append($"{functionName}");
          PrintFunctionArguments(methodCallExpression.Arguments.Append(Expression.Constant(ignoreNulls)));
        }

        break;
    }

    return methodCallExpression;
  }

  private string GetFunctionName(string methodName)
  {
    switch (methodName)
    {
      case nameof(IAggregations<object>.EarliestByOffset):
      case nameof(IAggregations<object>.EarliestByOffsetAllowNulls):
        return nameof(IAggregations<object>.EarliestByOffset).ToKSqlFunctionName();
      case nameof(IAggregations<object>.LatestByOffset):
      case nameof(IAggregations<object>.LatestByOffsetAllowNulls):
        return nameof(IAggregations<object>.LatestByOffset).ToKSqlFunctionName();
      default:
        throw new NotSupportedException();
    }
  }
}