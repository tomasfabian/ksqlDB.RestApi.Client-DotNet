namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

#nullable enable
public record Schema
{
  public string? Type { get; set; }
  public object? Fields { get; set; }
  public object? MemberSchema { get; set; }
}
