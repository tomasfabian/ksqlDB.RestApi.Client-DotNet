using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal class ConstantVisitor : KSqlVisitor
  {
    internal ConstantVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
      : base(stringBuilder, queryMetadata)
    {
    }

    public override Expression? Visit(Expression? expression)
    {
      if (expression == null)
        return null;

      switch (expression.NodeType)
      {
        case ExpressionType.Constant:
          VisitConstant((ConstantExpression) expression);
          break;

        default:
          base.Visit(expression);
          break;
      }

      return expression;
    }

    protected override Expression VisitConstant(ConstantExpression constantExpression)
    {
      if (constantExpression == null) throw new ArgumentNullException(nameof(constantExpression));

      var value = constantExpression.Value;
      var type = value?.GetType();

      if (value is byte[])
        throw new NotSupportedException();

      if (value is not string && QueryMetadata.IsInContainsScope && value is IEnumerable enumerable)
      {
        Append(enumerable);
      }
      else if (value != null && type != null && (type.IsClass || type.IsStruct() || type.IsDictionary()))
      {
        var ksqlValue = new CreateKSqlValue().ExtractValue(value, null, null, type, str => IdentifierUtil.Format(str, QueryMetadata.IdentifierEscaping));

        StringBuilder.Append(ksqlValue);
      }
      else if (KSqlDBContextOptions.NumberFormatInfo != null && value is double doubleValue)
      {
        var formatted = doubleValue.ToString(KSqlDBContextOptions.NumberFormatInfo);

        StringBuilder.Append(formatted);
      }
      else
        switch (value)
        {
          case ListSortDirection listSortDirection:
          {
            string direction = listSortDirection == ListSortDirection.Ascending ? "ASC" : "DESC";
            StringBuilder.Append($"'{direction}'");
            break;
          }
          case string:
            StringBuilder.Append($"'{value}'");
            break;
          default:
          {
            var stringValue = value != null ? value.ToString() : "NULL";
            StringBuilder.Append(stringValue ?? "Unknown");
            break;
          }
        }

      return constantExpression;
    }
  }
}
