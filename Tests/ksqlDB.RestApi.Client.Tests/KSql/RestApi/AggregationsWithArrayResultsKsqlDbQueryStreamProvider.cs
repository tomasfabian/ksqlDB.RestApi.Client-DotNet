using Kafka.DotNet.ksqlDB.KSql.RestApi;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi
{
  internal class AggregationsWithArrayResultsKsqlDbQueryStreamProvider : TestableKSqlDbQueryStreamProvider
  {
    public AggregationsWithArrayResultsKsqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory)
      : base(httpClientFactory)
    {
      QueryResponse =
        "{\"queryId\":\"d2caf633-58b2-4786-96cc-63b271a6bbb4\",\"columnNames\":[\"ID\",\"TOPK\"],\"columnTypes\":[\"INTEGER\",\"ARRAY<DOUBLE>\"]}\r\n[1,[4.2E-4]]\r\n[2,[1.0]]\r\n[1,[4.2E-4,4.2E-4]]";
    }
  }
}