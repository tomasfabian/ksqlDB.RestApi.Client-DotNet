using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors
{
  internal class ListInitVisitor : KSqlVisitor
  {
    private readonly KSqlQueryMetadata queryMetadata;

    internal ListInitVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
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
        case ExpressionType.ListInit:
          VisitListInit((ListInitExpression)expression);
          break;

        default:
          base.Visit(expression);
          break;
      }

      return expression;
    }

    protected override Expression VisitListInit(ListInitExpression listInitExpression)
    {
      var isDictionary = listInitExpression.Type.IsDictionary();

      if (isDictionary)
      {
        //MAP('c' := 2, 'd' := 4)
        Append("MAP(");

        bool isFirst = true;

        foreach (var elementInit in listInitExpression.Initializers)
        {
          if (isFirst)
            isFirst = false;
          else
            Append(ColumnsSeparator);

          Visit(elementInit.Arguments[0]);

          Append(" := ");
          Visit(elementInit.Arguments[1]);
        }

        Append(")");
      }
      else if (listInitExpression.Type.IsList())
      {
        var arguments = listInitExpression.Initializers.SelectMany(c => c.Arguments);

        if (queryMetadata.IsInContainsScope)
          JoinAppend(arguments);
        else
          PrintArray(arguments);
      }

      return listInitExpression;
    }
  }
}
