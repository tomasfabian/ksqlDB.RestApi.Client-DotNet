namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query.Descriptors;

public record Schema
{
  public string Type { get; set; } = null!;
  public object? Fields { get; set; }
  public object? MemberSchema { get; set; }
}
