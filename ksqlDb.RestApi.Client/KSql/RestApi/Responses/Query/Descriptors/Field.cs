namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record Field
{
  public string Name { get; set; }
  public Schema Schema { get; set; }
  public string Type { get; set; }
}