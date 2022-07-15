namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;

public record Query
{
  public string QueryString { get; set; }

  public string[] Sinks { get; set; }

  public string[] SinkKafkaTopics { get; set; }

  public string Id { get; set; }

  public StatusCount StatusCount { get; set; }

  public string QueryType { get; set; }

  public string State { get; set; }
}