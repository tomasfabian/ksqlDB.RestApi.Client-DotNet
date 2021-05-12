using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Kafka.DotNet.ksqlDB.KSql.Query.Functions;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal class KSqlFunctionVisitor : KSqlVisitor
  {
    public KSqlFunctionVisitor(StringBuilder stringBuilder, bool useTableAlias)
      : base(stringBuilder, useTableAlias)
    {
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
      var methodInfo = methodCallExpression.Method;

      if (methodCallExpression.Object == null
          && methodInfo.DeclaringType.Name == nameof(KSqlFunctionsExtensions))
      {
        switch (methodInfo.Name)
        {          
          case nameof(KSqlFunctionsExtensions.Dynamic):
            if (methodCallExpression.Arguments[1] is ConstantExpression constantExpression)
              Append($"{constantExpression.Value}");
            else
            {
              var value = ExtractFieldValue((MemberExpression)methodCallExpression.Arguments[1]);
              Append(value.ToString());
            }
            break;          
          case nameof(KSqlFunctionsExtensions.Random):
          case nameof(KSqlFunctionsExtensions.UnixDate):
          case nameof(KSqlFunctionsExtensions.UnixTimestamp):
            Append($"{methodInfo.Name.ToKSqlFunctionName()}()");
            break;
          case nameof(KSqlFunctionsExtensions.Like):
            Visit(methodCallExpression.Arguments[1]);
            Append(" LIKE ");
            Visit(methodCallExpression.Arguments[2]);
            break;
          case nameof(KSqlFunctionsExtensions.Trim):
          case nameof(KSqlFunctionsExtensions.Abs):
          case nameof(KSqlFunctionsExtensions.Ceil):
          case nameof(KSqlFunctionsExtensions.Ln):
          case nameof(KSqlFunctionsExtensions.Exp):
          case nameof(KSqlFunctionsExtensions.Floor):
          case nameof(KSqlFunctionsExtensions.Sign):
            Append($"{methodInfo.Name.ToUpper()}(");
            Visit(methodCallExpression.Arguments[1]);
            Append(")");
            break;
          case nameof(KSqlFunctionsExtensions.LPad):
          case nameof(KSqlFunctionsExtensions.Round):
          case nameof(KSqlFunctionsExtensions.Entries):
          case nameof(KSqlFunctionsExtensions.RPad):
          case nameof(KSqlFunctionsExtensions.Substring):
          case nameof(KSqlFunctionsExtensions.StringToDate):
          case nameof(KSqlFunctionsExtensions.DateToString):
          case nameof(KSqlFunctionsExtensions.StringToTimestamp):
          case nameof(KSqlFunctionsExtensions.TimestampToString):
            Append($"{methodInfo.Name.ToUpper()}");
            PrintFunctionArguments(methodCallExpression.Arguments.Skip(1));
            break;
          case nameof(KSqlFunctionsExtensions.GenerateSeries):
          case nameof(KSqlFunctionsExtensions.GeoDistance):
          case nameof(KSqlFunctionsExtensions.ArrayContains):
          case nameof(KSqlFunctionsExtensions.ArrayDistinct):
          case nameof(KSqlFunctionsExtensions.ArrayExcept):
          case nameof(KSqlFunctionsExtensions.ArrayIntersect):
          case nameof(KSqlFunctionsExtensions.ArrayJoin):
          case nameof(KSqlFunctionsExtensions.ArrayLength):
          case nameof(KSqlFunctionsExtensions.ArrayMax):
          case nameof(KSqlFunctionsExtensions.ArrayMin):
          case nameof(KSqlFunctionsExtensions.ArrayRemove):
            Append($"{methodInfo.Name.ToKSqlFunctionName()}");
            PrintFunctionArguments(methodCallExpression.Arguments.Skip(1));
            break;
        }
      }
      else base.VisitMethodCall(methodCallExpression);

      return methodCallExpression;
    }
  }
}