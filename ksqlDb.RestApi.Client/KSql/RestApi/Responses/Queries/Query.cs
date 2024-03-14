namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;

public record Query
{
  public string QueryString { get; set; } = null!;

  public string[] Sinks { get; set; } = null!;

  public string[] SinkKafkaTopics { get; set; } = null!;

  public string Id { get; set; } = null!;

  public StatusCount? StatusCount { get; set; }

  public string QueryType { get; set; } = null!;

  public string State { get; set; } = null!;
}
