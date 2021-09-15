namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Query.Descriptors
{
  public record Schema
  {
    public string Type { get; set; }
    public object Fields { get; set; }
    public object MemberSchema { get; set; }
  }
}