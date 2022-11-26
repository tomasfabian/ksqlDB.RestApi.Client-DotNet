namespace ksqlDb.RestApi.Client.KSql.Entities;

internal record FromItem
{
  public Type Type { get; set; }
  public string Name { get; set; }
  public string Alias { get; set; }
}