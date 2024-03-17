using ksqlDB.RestApi.Client.KSql.Query.Windows;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;

public record AssertTopicOptions
{
  public AssertTopicOptions(string topicName)
  {
    if (string.IsNullOrWhiteSpace(topicName))
      throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicName));

    TopicName = topicName;
  }

  /// The name of the kafka topic.
  public string TopicName { get; }

  /// Optional dictionary of topic properties. The only properties that will be checked are PARTITIONS and REPLICAS.
  public IDictionary<string, string>? Properties { get; set; }

  /// The TIMEOUT clause specifies the amount of time to wait for the assertion to succeed before failing. If the TIMEOUT clause is not present, then ksqlDB will use the timeout specified by the server configuration ksql.assert.topic.default.timeout.ms, which is 1000 ms by default.
  public Duration? Timeout { get; set; }
}
