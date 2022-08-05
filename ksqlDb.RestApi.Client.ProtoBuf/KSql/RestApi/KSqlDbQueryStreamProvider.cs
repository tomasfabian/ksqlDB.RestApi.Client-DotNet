using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using Microsoft.Extensions.Logging;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;

internal class KSqlDbQueryStreamProvider : KSqlDbQueryProvider
{
  public KSqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory, KSqlDbProviderOptions options, ILogger? logger = null) 
    : base(httpClientFactory, options, logger)
  {
  }

  protected override string QueryEndPointName => "query-stream";
}