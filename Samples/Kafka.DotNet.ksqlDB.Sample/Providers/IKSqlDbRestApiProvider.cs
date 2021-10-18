using System.Net.Http;
using System.Threading.Tasks;
using Kafka.DotNet.ksqlDB.KSql.RestApi;

namespace ksqlDB.Api.Client.Samples.Providers
{
  public interface IKSqlDbRestApiProvider : IKSqlDbRestApiClient
  {
    Task<HttpResponseMessage> DropStreamAndTopic(string streamName);
    Task<HttpResponseMessage> DropTableAndTopic(string tableName);
  }
}