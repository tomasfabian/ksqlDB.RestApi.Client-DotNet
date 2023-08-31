using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal class ConstantVisitor : KSqlVisitor
  {
    private readonly KSqlQueryMetadata queryMetadata;

    internal ConstantVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
      : base(stringBuilder, queryMetadata)
    {
      this.queryMetadata = queryMetadata ?? throw new ArgumentNullException(nameof(queryMetadata));
    }

    public override Expression Visit(Expression expression)
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

      if (value is not string && queryMetadata.IsInContainsScope && value is IEnumerable enumerable)
      {
        Append(enumerable);
      }
      else if (value != null && (type.IsClass || type.IsStruct() || type.IsDictionary()))
      {
        var ksqlValue = new CreateKSqlValue().ExtractValue(value, null, null, type);

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
