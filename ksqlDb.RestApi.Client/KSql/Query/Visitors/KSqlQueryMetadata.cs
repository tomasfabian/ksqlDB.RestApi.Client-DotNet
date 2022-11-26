using System.Linq.Expressions;
using ksqlDb.RestApi.Client.KSql.Entities;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal sealed record KSqlQueryMetadata
{
  public Type FromItemType { get; set; }
  public FromItem[] Joins { get; set; }

  internal LambdaExpression Select { get; set; }

  internal FromItem TrySetAlias(MemberExpression memberExpression, Func<FromItem, string, bool> predicate)
  {
    var parameterName = ((ParameterExpression)memberExpression.Expression).Name;

    var joinsOfType = Joins.Where(c => c.Type == memberExpression.Expression.Type).ToArray();

    var fromItem = joinsOfType.FirstOrDefault();

    if (joinsOfType.Length > 1)
      fromItem = joinsOfType.FirstOrDefault(c => predicate(c, parameterName));

    if (fromItem != null)
      fromItem.Alias = parameterName;

    return fromItem;
  }
}