using System.Linq.Expressions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.KSql.Entities;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal sealed record KSqlQueryMetadata
{
  internal ModelBuilder ModelBuilder { get; init; } = null!;

  internal EntityMetadata EntityMetadata { get; } = new();

  public Type FromItemType => EntityMetadata.Type;

  public FromItem[]? Joins { get; set; }

  internal LambdaExpression? Select { get; set; }

  internal bool IsInNestedFunctionScope { get; set; }

  internal bool IsInContainsScope { get; set; }

  public IdentifierEscaping IdentifierEscaping { get; init; } = IdentifierEscaping.Never;

  internal FromItem? TrySetAlias(MemberExpression memberExpression, Func<FromItem, string, bool> predicate)
  {
    var name = (memberExpression.Expression as ParameterExpression)?.Name;
    if (name == null)
      return null;

    var parameterName = IdentifierUtil.Format(name, IdentifierEscaping);

    var joinsOfType = Joins?.Where(c => c.Type == memberExpression.Expression?.Type).ToArray();

    var fromItem = joinsOfType?.FirstOrDefault();

    if (joinsOfType is {Length: > 1})
      fromItem = joinsOfType.FirstOrDefault(c => predicate(c, parameterName));

    if (fromItem != null)
      fromItem.Alias = parameterName;

    return fromItem;
  }
}
