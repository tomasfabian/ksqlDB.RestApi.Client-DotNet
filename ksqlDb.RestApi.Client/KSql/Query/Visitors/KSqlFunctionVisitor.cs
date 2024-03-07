using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Functions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class KSqlFunctionVisitor : KSqlVisitor
{
  public KSqlFunctionVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    var methodInfo = methodCallExpression.Method;

    if (methodCallExpression.Object == null
        && methodInfo.DeclaringType?.Name == nameof(KSqlFunctionsExtensions))
    {
      switch (methodInfo.Name)
      {          
        case nameof(KSqlFunctionsExtensions.Dynamic):
          if (methodCallExpression.Arguments[1] is ConstantExpression constantExpression)
            Append($"{constantExpression.Value}");
          else
          {
            var value = ExtractMemberValue((MemberExpression)methodCallExpression.Arguments[1]);
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
        case nameof(KSqlFunctionsExtensions.Sqrt):
        case nameof(KSqlFunctionsExtensions.Sign):
          Append($"{methodInfo.Name.ToUpper()}(");
          Visit(methodCallExpression.Arguments[1]);
          Append(")");
          break;
        case nameof(KSqlFunctionsExtensions.Explode):
          Append($"{methodInfo.Name.ToUpper()}(");
          Visit(methodCallExpression.Arguments[0]);
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
        case nameof(KSqlFunctionsExtensions.ExtractJsonField):
        case nameof(KSqlFunctionsExtensions.Encode):
        case nameof(KSqlFunctionsExtensions.InitCap):
        case nameof(KSqlFunctionsExtensions.IfNull):
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
        case nameof(KSqlFunctionsExtensions.ArraySort):
        case nameof(KSqlFunctionsExtensions.ArrayUnion):
        case nameof(KSqlFunctionsExtensions.AsMap):
        case nameof(KSqlFunctionsExtensions.JsonArrayContains):
        case nameof(KSqlFunctionsExtensions.IsJsonString):
        case nameof(KSqlFunctionsExtensions.JsonArrayLength):
        case nameof(KSqlFunctionsExtensions.JsonKeys):
        case nameof(KSqlFunctionsExtensions.JsonRecords):
        case nameof(KSqlFunctionsExtensions.ToJsonString):
        case nameof(KSqlFunctionsExtensions.MapKeys):
        case nameof(KSqlFunctionsExtensions.ToBytes):
        case nameof(KSqlFunctionsExtensions.FromBytes):
        case nameof(KSqlFunctionsExtensions.Instr):
        case nameof(KSqlFunctionsExtensions.FormatDate):
        case nameof(KSqlFunctionsExtensions.FormatTime):
        case nameof(KSqlFunctionsExtensions.ParseDate):
        case nameof(KSqlFunctionsExtensions.ParseTime):
          Append($"{methodInfo.Name.ToKSqlFunctionName()}");
          PrintFunctionArguments(methodCallExpression.Arguments.Skip(1));
          break;
        case nameof(KSqlFunctionsExtensions.Concat):
        case nameof(KSqlFunctionsExtensions.JsonConcat):
          Append($"{methodInfo.Name.ToKSqlFunctionName()}");
          var newArrayExpression = methodCallExpression.Arguments.Skip(1).OfType<NewArrayExpression>().First();
          VisitParams(newArrayExpression);
          break;
        case nameof(KSqlFunctionsExtensions.ConcatWS):
          PrintConcatWithSeparator(methodCallExpression);
          break;
      }
    }
    else base.VisitMethodCall(methodCallExpression);

    return methodCallExpression;
  }

  private void PrintConcatWithSeparator(MethodCallExpression methodCallExpression)
  {
    Append("CONCAT_WS(");
    Visit(methodCallExpression.Arguments[1]);
    Append(", ");
    var newArrayExpression2 = methodCallExpression.Arguments.Skip(1).OfType<NewArrayExpression>().First();
    PrintCommaSeparated(newArrayExpression2.Expressions);

    Append(")");
  }

  protected void VisitParams(NewArrayExpression node)
  {
    Append("(");

    PrintCommaSeparated(node.Expressions);
      
    Append(")");
  }
}
