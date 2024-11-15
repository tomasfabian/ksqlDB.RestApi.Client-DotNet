using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using Microsoft.Extensions.Logging;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;

internal class KSqlDbQueryStreamProvider : KSqlDbQueryProvider
{
  public KSqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory, IMetadataProvider metadataProvider, KSqlDbProviderOptions options, ILogger? logger = null)
    : base(httpClientFactory, metadataProvider, options, logger)
  {
  }

  protected override string QueryEndPointName => "query-stream";
}
