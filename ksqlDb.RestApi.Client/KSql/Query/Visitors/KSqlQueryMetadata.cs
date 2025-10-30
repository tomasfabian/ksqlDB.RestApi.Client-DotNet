using System.Linq.Expressions;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.KSql.Entities;
using ksqlDb.RestApi.Client.KSql.Query.Metadata;
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

  public IDictionary<string, List<AnonymousTypeMapping>> NewAnonymousTypeMappings = new Dictionary<string, List<AnonymousTypeMapping>>();

  internal FromItem? TrySetAlias(MemberExpression memberExpression, Func<FromItem, string, bool> predicate)
  {
    var name = (memberExpression.Expression as ParameterExpression)?.Name;
    if (name == null)
      return null;

    var fromItem = GetMappedFromItem(memberExpression);
    if (fromItem != null)
    {
      return fromItem;
    }

    var alias = IdentifierUtil.Format(name, IdentifierEscaping);

    var joinsOfType = Joins?.Where(c => c.Type == memberExpression.Expression?.Type).ToArray();
    fromItem = joinsOfType?.FirstOrDefault();

    if (joinsOfType is {Length: > 1})
      fromItem = joinsOfType.FirstOrDefault(c => predicate(c, alias));

    if (fromItem != null)
      fromItem.Alias = alias;

    return fromItem;
  }

  private FromItem? GetMappedFromItem(MemberExpression memberExpression)
  {
    if (memberExpression.Expression == null || !memberExpression.Expression.Type.IsAnonymousType())
    {
      return null;
    }

    var mappingKey = memberExpression.Member.Name;
    if (NewAnonymousTypeMappings.TryGetValue(mappingKey, out var mappings))
    {
      var mapping = mappings.First();
      var join = Joins?.FirstOrDefault(c => c.Type == mapping.DeclaringType && !string.IsNullOrEmpty(c.Alias));
      if (join != null && !string.IsNullOrEmpty(join.Alias))
      {
        return join;
      }
    }

    return null;
  }

  internal string? GetFromItemAlias()
  {
    return Joins?.Where(c => c.Type == FromItemType && !string.IsNullOrEmpty(c.Alias)).Select(c => c.Alias).LastOrDefault();
  }
}
