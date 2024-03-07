using ksqlDb.RestApi.Client.IntegrationTests.Helpers;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using HttpClientFactory = ksqlDb.RestApi.Client.IntegrationTests.Http.HttpClientFactory;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDb.RestApi.Client.IntegrationTests.KSql.RestApi;

public class KSqlDbRestApiProvider(IHttpClientFactory httpClientFactory) : KSqlDbRestApiClient(httpClientFactory)
{
  internal static string KsqlDbUrl => TestConfig.KSqlDbUrl;

  public static KSqlDbRestApiProvider Create(string? ksqlDbUrl = null)
  {
    var uri = new Uri(ksqlDbUrl ?? KsqlDbUrl);

    return new KSqlDbRestApiProvider(new HttpClientFactory(uri));
  }

  public Task<HttpResponseMessage> DropStreamAndTopic(string streamName)
  {
    var statement = $"DROP STREAM IF EXISTS {streamName} DELETE TOPIC;";
      
    KSqlDbStatement ksqlDbStatement = new(statement);

    return ExecuteStatementAsync(ksqlDbStatement);
  }

  public Task<HttpResponseMessage> DropTableAndTopic(string tableName)
  {
    var statement = $"DROP TABLE IF EXISTS {tableName} DELETE TOPIC;";
      
    KSqlDbStatement ksqlDbStatement = new(statement);

    return ExecuteStatementAsync(ksqlDbStatement);
  }
}
