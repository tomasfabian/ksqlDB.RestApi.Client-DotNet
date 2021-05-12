using Kafka.DotNet.ksqlDB.KSql.RestApi;

namespace Kafka.DotNet.ksqlDB.Tests.Extensions.KSql.RestApi
{
  internal class MapResultsKsqlDbQueryStreamProvider : TestableKSqlDbQueryStreamProvider
  {
    public MapResultsKsqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory)
      : base(httpClientFactory)
    {
      QueryResponse =
        "{\"queryId\":\"713207d7-8772-4f03-a3a6-b8f506f784db\",\"columnNames\":[\"KSQL_COL_0\"],\"columnTypes\":[\"MAP<STRING, INTEGER>\"]}\r\n[{\"a\":1,\"b\":2}]\r\n[{\"d\":4,\"c\":2}]";
    }
  }
}