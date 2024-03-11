using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Statements;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Responses.Asserts;

#nullable enable
public record AssertTopicResponse : StatementResponseBase
{
  public string? TopicName { get; set; }
  public bool Exists { get; set; }
}
