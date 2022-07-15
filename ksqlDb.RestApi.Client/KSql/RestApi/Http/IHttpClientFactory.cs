using System.Net.Http;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Http;

public interface IHttpClientFactory
{
  HttpClient CreateClient();
}