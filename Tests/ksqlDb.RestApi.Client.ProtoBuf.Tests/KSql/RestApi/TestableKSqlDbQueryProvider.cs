using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;
using Moq.Protected;
using Moq;
using System.Net;

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
  protected string QueryResponse = "[{\"header\":{\"queryId\":\"transient_MOVIES_7139331784417618909\",\"schema\":\"`ID` INTEGER, `TITLE` STRING, `RELEASE_YEAR` INTEGER, `ROWTIME` BIGINT\",\"protoSchema\":\"syntax = \\\"proto3\\\";\\n\\nmessage ConnectDefault1 {\\n  int32 ID = 1;\\n  string TITLE = 2;\\n  int32 RELEASE_YEAR = 3;\\n  int64 ROWTIME = 4;\\n}\\n\"}},";
  protected string ItemResponse = "{\"row\":{\"protobufBytes\":\"CAESBkFsaWVucxjCDyCrw6nKwzA=\"}},";

  protected override HttpClient OnCreateHttpClient()
  {     
    return FakeHttpClient.CreateWithResponse(QueryResponse);
  }
}
public static class FakeHttpClient
{
  public static Mock<HttpMessageHandler> CreateHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
  {
    var handlerMock = new Mock<HttpMessageHandler>();

    handlerMock
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        nameof(HttpClient.SendAsync),
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
      )
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = statusCode,
        Content = new StringContent(responseContent),
      })
      .Verifiable();

    return handlerMock;
  }

  public static HttpClient ToHttpClient(this Mock<HttpMessageHandler> handlerMock)
  {
    return new HttpClient(handlerMock.Object)
    {
      BaseAddress = new Uri(TestableKSqlDbQueryProvider.KsqlDbUrl)
    };
  }

  public static HttpClient CreateWithResponse(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
  {
    var handlerMock = CreateHttpMessageHandler(responseContent, statusCode);

    return handlerMock.ToHttpClient();
  }
}