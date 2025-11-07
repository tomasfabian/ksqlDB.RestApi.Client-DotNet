namespace ksqlDb.RestApi.Client.KSql.Query.Metadata;

internal sealed record AnonymousTypeMapping
{
  public string PropertyName { get; init; } = null!;
  public Type? DeclaringType { get; init; }
  public string? ParameterName { get; set; }
  public string? ProjectedName { get; set; } = null!;
}
