using System.Net.Http;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi
{
  public interface IHttpClientFactory
  {
    HttpClient CreateClient();
  }
}