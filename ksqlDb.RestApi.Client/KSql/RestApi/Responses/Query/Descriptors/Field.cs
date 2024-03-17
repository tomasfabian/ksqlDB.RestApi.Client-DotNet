namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record Field
{
  public string Name { get; set; } = null!;
  public Schema Schema { get; set; } = null!;
  public string Type { get; set; } = null!;
}
