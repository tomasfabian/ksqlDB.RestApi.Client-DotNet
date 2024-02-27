using System.Linq.Expressions;
using ksqlDb.RestApi.Client.KSql.Entities;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal sealed record KSqlQueryMetadata
{
  public Type FromItemType { get; set; }
  public FromItem[] Joins { get; set; }

  internal LambdaExpression Select { get; set; }

  internal bool IsInNestedFunctionScope { get; set; }

  internal bool IsInContainsScope { get; set; }
  public IdentifierFormat IdentifierFormat { get; init; } = IdentifierFormat.None;

  internal FromItem TrySetAlias(MemberExpression memberExpression, Func<FromItem, string, bool> predicate)
  {
    var parameterName = IdentifierUtil.Format(((ParameterExpression)memberExpression.Expression).Name, IdentifierFormat);

    var joinsOfType = Joins.Where(c => c.Type == memberExpression.Expression.Type).ToArray();

    var fromItem = joinsOfType.FirstOrDefault();

    if (joinsOfType.Length > 1)
      fromItem = joinsOfType.FirstOrDefault(c => predicate(c, parameterName));

    if (fromItem != null)
      fromItem.Alias = parameterName;

    return fromItem;
  }
}
