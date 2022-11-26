using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class StringVisitor : KSqlVisitor
{
  public StringVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    if (methodCallExpression.Object?.Type != typeof(string)) 
      return methodCallExpression;

    var methodInfo = methodCallExpression.Method;

    switch (methodInfo.Name)
    {
      case nameof(string.ToUpper):
        Append("UCASE(");
        Visit(methodCallExpression.Object);
        Append(")");
        break;
      case nameof(string.ToLower):
        Append("LCASE(");
        Visit(methodCallExpression.Object);
        Append(")");
        break;
      case nameof(string.StartsWith):
      case nameof(string.Contains):
      case nameof(string.EndsWith):
        VisitLike(methodCallExpression, methodInfo.Name);
        break;
    }

    return methodCallExpression;
  }

  #region Like

  private void AppendLike(MethodCallExpression methodCallExpression)
  {
    Visit(methodCallExpression.Object);

    Append(" LIKE ");
  }

  private string likeMethodName;

  private void VisitLike(MethodCallExpression methodCallExpression, string methodName)
  {
    likeMethodName = methodName;

    AppendLike(methodCallExpression);

    Visit(methodCallExpression.Arguments[0]);

    likeMethodName = null;
  }

  #endregion

  protected override Expression VisitParameter(ParameterExpression node)
  {
    Append(node.Name);

    return base.VisitParameter(node);
  }

  protected override Expression VisitConstant(ConstantExpression constantExpression)
  {
    if (likeMethodName.IsNotNullOrEmpty() && constantExpression.Value is string value)
    {
      Append("'");

      if (likeMethodName.IsOneOfFollowing(nameof(String.EndsWith), nameof(String.Contains)))
        Append("%");

      Append(value);

      if (likeMethodName.IsOneOfFollowing(nameof(String.StartsWith), nameof(String.Contains)))
        Append("%");

      Append("'");

      return constantExpression;
    }

    return base.VisitConstant(constantExpression);
  }
}