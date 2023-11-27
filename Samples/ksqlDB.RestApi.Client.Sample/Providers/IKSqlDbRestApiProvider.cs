using ksqlDB.RestApi.Client.KSql.RestApi;

namespace ksqlDB.RestApi.Client.Samples.Providers;

public interface IKSqlDbRestApiProvider : IKSqlDbRestApiClient
{
  Task<HttpResponseMessage> DropStreamAndTopic(string streamName);
  Task<HttpResponseMessage> DropTableAndTopic(string tableName);
}