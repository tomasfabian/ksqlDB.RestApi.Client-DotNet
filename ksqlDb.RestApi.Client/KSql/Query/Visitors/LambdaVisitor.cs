using System.Linq.Expressions;
using System.Text;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class LambdaVisitor : KSqlVisitor
{
  public LambdaVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
  }

  protected override KSqlFunctionVisitor CreateKSqlFunctionVisitor()
  {
    return new KSqlFunctionLambdaVisitor(StringBuilder, QueryMetadata);
  }

  public override Expression? Visit(Expression? expression)
  {
    if (expression == null)
      return null;

    switch (expression.NodeType)
    {
      case ExpressionType.Lambda:
        base.Visit(expression);
        break;
        
      case ExpressionType.MemberAccess:
        VisitMember((MemberExpression)expression);
        break;
        
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
    if (node.Name != null)
      Append(node.Name);

    return base.VisitParameter(node);
  }

  protected override Expression VisitMember(MemberExpression memberExpression)
  {
    if (memberExpression == null) throw new ArgumentNullException(nameof(memberExpression));

    var memberName = memberExpression.Member.Name;
    
    switch (memberExpression.Expression?.NodeType)
    {
      case ExpressionType.MemberInit:
        Destructure(memberExpression);
        break;
      case ExpressionType.MemberAccess when memberName == nameof(string.Length):
        Append("LEN(");
        Visit(memberExpression.Expression);
        Append(")");
        break;
      case ExpressionType.MemberAccess when memberExpression.NodeType == ExpressionType.MemberAccess:
        Destructure(memberExpression);
        break;
      case ExpressionType.MemberAccess:
        Append($"{memberExpression.Member.Name.ToUpper()}");
        break;
      case ExpressionType.Parameter when memberExpression.NodeType == ExpressionType.MemberAccess:
        Destructure(memberExpression);
        break;
      case ExpressionType.Parameter:
        Append(memberExpression.Member.Name);
        break;
      default:
      {
        var outerObj = ExtractMemberValue(memberExpression);

        Visit(Expression.Constant(outerObj));
        break;
      }
    }
    
    return memberExpression;
  }

  protected override Expression VisitLambda<T>(Expression<T> node)
  {
    Append("(");

    bool isFirst = true;

    foreach (var parameterExpression in node.Parameters)
    {
      if (isFirst)
        isFirst = false;
      else
        Append(", ");

      Append(parameterExpression.Name!);
    }
      
    Append(") => ");

    QueryMetadata.IsInNestedFunctionScope = true;

    Visit(node.Body);

    QueryMetadata.IsInNestedFunctionScope = false;

    return node;
  }
}
