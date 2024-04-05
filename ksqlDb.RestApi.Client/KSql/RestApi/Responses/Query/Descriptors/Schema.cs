namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record Schema
{
  /// <summary>
  /// The type the schema represents.
  /// </summary>
  public string Type { get; set; } = null!;

  /// <summary>
  /// For STRUCT types, contains a list of field objects that describes each field within the struct. For other types this field is not used and its value is undefined.
  /// </summary>
  public object? Fields { get; set; }

  /// <summary>
  /// A schema object. For MAP and ARRAY types, contains the schema of the map values and array elements, respectively.
  /// </summary>
  public object? MemberSchema { get; set; }
}
