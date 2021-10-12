using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Kafka.DotNet.ksqlDB.KSql.Query.Visitors
{
  internal class LambdaVisitor : KSqlVisitor
  {
    private readonly StringBuilder stringBuilder;

    public LambdaVisitor(StringBuilder stringBuilder)
      : base(stringBuilder)
    {
      this.stringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
    }

    protected override KSqlFunctionVisitor CreateKSqlFunctionVisitor()
    {
      return new KSqlFunctionLambdaVisitor(stringBuilder);
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
      Append(node.Name);

      return base.VisitParameter(node);
    }

    protected override Expression VisitMember(MemberExpression memberExpression)
    {
      if (memberExpression == null) throw new ArgumentNullException(nameof(memberExpression));

      var memberName = memberExpression.Member.Name;

      if (memberExpression.Expression.NodeType == ExpressionType.MemberInit)
      {
        Destructure(memberExpression);
      }
      else if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
      {
        if (memberName == nameof(string.Length))
        {
          Append("LEN(");
          Visit(memberExpression.Expression);
          Append(")");
        }
        else if(memberExpression.NodeType == ExpressionType.MemberAccess)
        {
          Destructure(memberExpression);
        }
        else
          Append($"{memberExpression.Member.Name.ToUpper()}");
      }
      else if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
      {
        if(memberExpression.NodeType == ExpressionType.MemberAccess)
          Destructure(memberExpression);
        else
          Append(memberExpression.Member.Name);
      }
      else
      {
        var outerObj = ExtractFieldValue(memberExpression);

        Visit(Expression.Constant(outerObj));
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

        Append(parameterExpression.Name);
      }
      
      Append(") => ");

      Visit(node.Body);

      return node;
    }
  }
}