using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;
using ksqlDb.RestApi.Client.ProtoBuf.Tests.Helpers;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.RestApi;

internal class TestableKSqlDbQueryProvider(IHttpV1ClientFactory httpClientFactory)
  : KSqlDbQueryProvider(httpClientFactory, new ModelBuilder(), KSqlDbContextOptionsInstance)
{
  public static readonly KSqlDBContextOptions KSqlDbContextOptionsInstance = new(TestParameters.KsqlDbUrl)
  {
    JsonSerializerOptions = KSqlDbJsonSerializerOptions.CreateInstance()
  };

  protected string QueryResponse =
    "[{\"header\":{\"queryId\":\"transient_MOVIES_8790538776625545898\",\"schema\":\"`ID` INTEGER, `TITLE` STRING, `RELEASE_YEAR` INTEGER\",\"protoSchema\":\"syntax = \\\"proto3\\\";\\n\\nmessage ConnectDefault1 {\\n  int32 ID = 1;\\n  string TITLE = 2;\\n  int32 RELEASE_YEAR = 3;\\n}\\n\"}}," +
    "\r\n{\"row\":{\"protobufBytes\":\"CgZBbGllbnMQARjCDw==\"}},"+
    "\r\n{\"row\":{\"protobufBytes\":\"CghEaWUgSGFyZBACGM4P\"}},";

  protected override HttpClient OnCreateHttpClient()
  {     
    return FakeHttpClient.CreateWithResponse(QueryResponse);
  }
}
