using System.Net;
using ksqlDB.Api.Client.Tests.Helpers;
using ksqlDB.Api.Client.Tests.Helpers.Http;
using ksqlDB.RestApi.Client.KSql.RestApi;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDB.Api.Client.Tests.KSql.RestApi;

internal class TestableKSqlDbQueryStreamProvider : KSqlDbQueryStreamProvider
{
  public TestableKSqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory, ILogger? logger = null)
    : base(httpClientFactory, TestKSqlDBContextOptions.Instance, logger)
  {
  }

  public bool ShouldThrowException { get; set; }

  protected string QueryResponse =
    @"{""queryId"":""59df818e-7d88-436f-95ac-3c59becc9bfb"",""columnNames"":[""ROWTIME"",""MESSAGE"",""ID"",""ISROBOT"",""ACCOUNTBALANCE"",""AMOUNT""],""columnTypes"":[""BIGINT"",""STRING"",""INTEGER"",""BOOLEAN"",""DECIMAL(16, 4)"",""DOUBLE""]}
[1611327570881,""Hello world"",1,true,9999999999999999.1234,4.2E-4]
[1611327671476,""Wall-e"",2,false,1.2000,1.0]";

  protected string ErrorResponse =
    @"{""@type"":""generic_error"",""error_code"":40001,""message"":""Line: 1, Col: 21: SELECT column 'Foo' cannot be resolved.\nStatement: SELECT Message, Id, Foo FROM Tweets\r\nWHERE Message = 'Hello world' EMIT CHANGES LIMIT 2;""}";

  internal IsDisposedHttpClient LastUsedHttpClient { get; private set; } = null!;

  protected override HttpClient OnCreateHttpClient()
  {
    var handlerMock = new Mock<HttpMessageHandler>();

    handlerMock
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        nameof(HttpClient.SendAsync),
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>()
      )
      .ReturnsAsync(new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(ShouldThrowException ? ErrorResponse : QueryResponse),
      })
      .Verifiable();

    return LastUsedHttpClient = new IsDisposedHttpClient(handlerMock.Object)
    {
      BaseAddress = new Uri(TestParameters.KsqlDBUrl)
    };
  }

  public Exception Exception { get; set; } = null!;

  protected override HttpRequestMessage CreateQueryHttpRequestMessage(HttpClient httpClient, object parameters)
  {
    if (Exception != null)
      throw Exception;

    return base.CreateQueryHttpRequestMessage(httpClient, parameters);
  }
}
