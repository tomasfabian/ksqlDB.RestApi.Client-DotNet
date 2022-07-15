using System.Linq.Expressions;
using System.Text;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class KSqlFunctionLambdaVisitor : KSqlFunctionVisitor
{
  public KSqlFunctionLambdaVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata) 
    : base(stringBuilder, queryMetadata)
  {
  }
    
  public override Expression Visit(Expression expression)
  {
    if (expression == null)
      return null;

    switch (expression.NodeType)
    {
      case ExpressionType.Parameter:
        VisitParameter((ParameterExpression)expression);
        break;
      default:
        base.Visit(expression);
        break;
    }

    return expression;
  }

  protected override Expression VisitParameter(ParameterExpression node)
  {
    Append(node.Name);

    return node;
  }
}