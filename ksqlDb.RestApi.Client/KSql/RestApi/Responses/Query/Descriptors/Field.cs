namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record Field
{
  /// <summary>
  /// The name of the field.
  /// </summary>
  public string Name { get; set; } = null!;

  /// <summary>
  /// A schema object that describes the schema of the field.
  /// </summary>
  public Schema Schema { get; set; } = null!;
  
  public string Type { get; set; } = null!;
}
