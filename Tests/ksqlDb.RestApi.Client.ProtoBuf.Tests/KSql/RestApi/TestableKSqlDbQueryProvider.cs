using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;

namespace ksqlDb.RestApi.Client.ProtoBuf.Tests.KSql.RestApi;

internal class TestableKSqlDbQueryProvider : KSqlDbQueryProvider
{
  internal const string KsqlDbUrl = @"http:\\localhost:8088";

  public static readonly KSqlDBContextOptions KSqlDbContextOptionsInstance = new(KsqlDbUrl)
  {
    JsonSerializerOptions =
      ksqlDb.RestApi.Client.KSql.Query.Context.Options.KSqlDbJsonSerializerOptions.CreateInstance()
  };

  public TestableKSqlDbQueryProvider(IHttpV1ClientFactory httpClientFactory)
    : base(httpClientFactory, KSqlDbContextOptionsInstance)
  {
  }

  protected string QueryResponse =
    "[{\"header\":{\"queryId\":\"transient_MOVIES_8790538776625545898\",\"schema\":\"`ID` INTEGER, `TITLE` STRING, `RELEASE_YEAR` INTEGER\",\"protoSchema\":\"syntax = \\\"proto3\\\";\\n\\nmessage ConnectDefault1 {\\n  int32 ID = 1;\\n  string TITLE = 2;\\n  int32 RELEASE_YEAR = 3;\\n}\\n\"}}," +
    "\r\n{\"row\":{\"protobufBytes\":\"CgZBbGllbnMQARjCDw==\"}},"+
    "\r\n{\"row\":{\"protobufBytes\":\"CghEaWUgSGFyZBACGM4P\"}},";

  protected override HttpClient OnCreateHttpClient()
  {     
    return FakeHttpClient.CreateWithResponse(QueryResponse);
  }
}