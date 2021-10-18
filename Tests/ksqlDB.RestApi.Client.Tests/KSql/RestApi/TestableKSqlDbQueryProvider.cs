using System.Net.Http;
using Kafka.DotNet.ksqlDB.KSql.RestApi;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Query;
using ksqlDB.Api.Client.Tests.Fakes.Http;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi
{
  internal class TestableKSqlDbQueryProvider : KSqlDbQueryProvider
  {
    public TestableKSqlDbQueryProvider(IHttpClientFactory httpClientFactory)
      : base(httpClientFactory)
    {
    }

    protected string QueryResponse = "[{\"header\":{\"queryId\":\"_confluent-ksql-default_transient_9174388154324047204_1614627435343\",\"schema\":\"`ID` INTEGER, `ARR` ARRAY<STRUCT<`TITLE` STRING, `ID` INTEGER>>, `MAPVALUE` MAP<STRING, MAP<STRING, INTEGER>>, `MAPARR` MAP<INTEGER, ARRAY<STRING>>, `STR` STRUCT<`TITLE` STRING, `ID` INTEGER>, `RELEASE_YEAR` INTEGER\"}},\r\n{\"row\":{\"columns\":[1,[{\"TITLE\":\"Aliens\",\"ID\":1},{\"TITLE\":\"test\",\"ID\":2}],{\"a\":{\"a\":1,\"b\":2},\"b\":{\"d\":4,\"c\":3}},{\"1\":[\"a\",\"b\"],\"2\":[\"c\",\"d\"]},{\"TITLE\":\"Aliens\",\"ID\":1},1986]}},\r\n{\"row\":{\"columns\":[2,[{\"TITLE\":\"Die Hard\",\"ID\":2},{\"TITLE\":\"test\",\"ID\":2}],{\"a\":{\"a\":1,\"b\":2},\"b\":{\"d\":4,\"c\":3}},{\"1\":[\"a\",\"b\"],\"2\":[\"c\",\"d\"]},{\"TITLE\":\"Die Hard\",\"ID\":2},1998]}},";

    protected override HttpClient OnCreateHttpClient()
    {     
      return FakeHttpClient.CreateWithResponse(QueryResponse);;
    }
  }
}