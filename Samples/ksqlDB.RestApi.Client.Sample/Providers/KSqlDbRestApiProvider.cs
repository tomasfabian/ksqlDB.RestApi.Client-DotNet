using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using Microsoft.Extensions.Logging;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.Api.Client.Samples.Providers;

public class KSqlDbRestApiProvider : KSqlDbRestApiClient, IKSqlDbRestApiProvider
{
  public KSqlDbRestApiProvider(IHttpClientFactory httpClientFactory, ILoggerFactory? loggerFactory = null)
    : base(httpClientFactory, loggerFactory)
  {
  }

  public static string KsqlDbUrl { get; } = @"http:\\localhost:8088";

  public Task<HttpResponseMessage> DropStreamAndTopic(string streamName)
  {
    return DropStreamAsync(streamName, true, true);
  }

  public Task<HttpResponseMessage> DropTableAndTopic(string tableName)
  {
    return DropTableAsync(tableName, true, true);
  }

  public static KSqlDbRestApiProvider Create(string? ksqlDbUrl = null)
  {
    var uri = new Uri(ksqlDbUrl ?? KsqlDbUrl);

    var httpClient = new HttpClient
    {
      BaseAddress = uri
    };

    return new KSqlDbRestApiProvider(new HttpClientFactory(httpClient));
  }
}