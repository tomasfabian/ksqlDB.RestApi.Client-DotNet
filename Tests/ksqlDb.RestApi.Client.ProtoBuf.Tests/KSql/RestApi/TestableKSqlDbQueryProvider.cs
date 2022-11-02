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

  protected string QueryResponse = "[{\"header\":{\"queryId\":\"_confluent-ksql-default_transient_9174388154324047204_1614627435343\",\"schema\":\"`ID` INTEGER, `ARR` ARRAY<STRUCT<`TITLE` STRING, `ID` INTEGER>>, `MAPVALUE` MAP<STRING, MAP<STRING, INTEGER>>, `MAPARR` MAP<INTEGER, ARRAY<STRING>>, `STR` STRUCT<`TITLE` STRING, `ID` INTEGER>, `RELEASE_YEAR` INTEGER\"}},\r\n{\"row\":{\"columns\":[1,[{\"TITLE\":\"Aliens\",\"ID\":1},{\"TITLE\":\"test\",\"ID\":2}],{\"a\":{\"a\":1,\"b\":2},\"b\":{\"d\":4,\"c\":3}},{\"1\":[\"a\",\"b\"],\"2\":[\"c\",\"d\"]},{\"TITLE\":\"Aliens\",\"ID\":1},1986]}},\r\n{\"row\":{\"columns\":[2,[{\"TITLE\":\"Die Hard\",\"ID\":2},{\"TITLE\":\"test\",\"ID\":2}],{\"a\":{\"a\":1,\"b\":2},\"b\":{\"d\":4,\"c\":3}},{\"1\":[\"a\",\"b\"],\"2\":[\"c\",\"d\"]},{\"TITLE\":\"Die Hard\",\"ID\":2},1998]}},";

  protected override HttpClient OnCreateHttpClient()
  {     
    return FakeHttpClient.CreateWithResponse(QueryResponse);;
  }
}
public static class FakeHttpClient
{
  public static Mock<DelegatingHandler> CreateDelegatingHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
  {
    var handlerMock = new Mock<DelegatingHandler>();

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