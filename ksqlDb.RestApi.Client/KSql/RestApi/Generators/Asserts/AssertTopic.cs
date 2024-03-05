namespace ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;

internal class AssertTopic
{
  public static string CreateStatement(bool exists, AssertTopicOptions options)
  {
    var notExists = exists ? string.Empty : "NOT EXISTS ";

    var withProperties = options.Properties != null ? CreateWith(options.Properties) : string.Empty;

    var timeOut = options.Timeout != null ? $" TIMEOUT {options.Timeout.Value} {options.Timeout.TimeUnit}" : string.Empty;

    var statement = $"ASSERT {notExists}TOPIC {options.TopicName}{withProperties}{timeOut};";

    return statement;
  }

  private static string CreateWith(IDictionary<string, string> properties)
  {
    var keyValueProperties = properties.Select(c => $"{c.Key}={c.Value}");

    string withClause = string.Join(", ", keyValueProperties);

    if (!string.IsNullOrEmpty(withClause))
      withClause = $" WITH ( {withClause} )";

    return withClause;
  }
}
