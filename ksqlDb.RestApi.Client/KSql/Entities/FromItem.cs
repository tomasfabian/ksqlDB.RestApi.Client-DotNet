namespace ksqlDb.RestApi.Client.KSql.Entities;

/// <summary>
/// Provides information about the data sources e.g. streams and tables, their types, names and aliases.
/// </summary>
internal record FromItem
{
  public Type Type { get; set; }
  public string Name { get; set; }
  public string Alias { get; set; }
}
