using ksqlDB.RestApi.Client.KSql.Query.Windows;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Generators.Asserts;

internal class AssertTopic
{
  public static string CreateStatement(bool exists, string topicName, IDictionary<string, string> properties = null, Duration timeout = null)
  {
    var notExists = exists ? string.Empty : "NOT EXISTS ";

    var withProperties = properties != null ? CreateWith(properties) : string.Empty;

    var timeOut = timeout != null ? $" TIMEOUT {timeout.Value} {timeout.TimeUnit}" : string.Empty;

    var statement = $@"ASSERT {notExists}TOPIC {topicName}{withProperties}{timeOut};";

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