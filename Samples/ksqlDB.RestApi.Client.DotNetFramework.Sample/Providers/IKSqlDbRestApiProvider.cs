using System.Net.Http;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.RestApi;

namespace ksqlDB.RestApi.Client.DotNetFramework.Sample.Providers;

public interface IKSqlDbRestApiProvider : IKSqlDbRestApiClient
{
  Task<HttpResponseMessage> DropStreamAndTopic(string streamName);
  Task<HttpResponseMessage> DropTableAndTopic(string tableName);
}
