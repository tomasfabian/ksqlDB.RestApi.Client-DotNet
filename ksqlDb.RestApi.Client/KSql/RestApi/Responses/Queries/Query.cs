namespace ksqlDB.RestApi.Client.KSql.RestApi.Responses.Queries;

public record Query
{
  /// <summary>
  /// The text of the statement that started the query.
  /// </summary>
  public string QueryString { get; set; } = null!;

  /// <summary>
  /// The streams and tables being written to by the query.
  /// </summary>
  public string[] Sinks { get; set; } = null!;

  public string[] SinkKafkaTopics { get; set; } = null!;

  /// <summary>
  /// The query ID.
  /// </summary>
  public string Id { get; set; } = null!;

  public StatusCount? StatusCount { get; set; }

  public string QueryType { get; set; } = null!;

  public string State { get; set; } = null!;
}
