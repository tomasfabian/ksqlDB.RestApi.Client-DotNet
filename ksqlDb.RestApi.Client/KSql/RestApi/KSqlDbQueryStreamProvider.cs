#if !NETSTANDARD
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;

namespace ksqlDB.RestApi.Client.KSql.RestApi
{
  internal class KSqlDbQueryStreamProvider : KSqlDbProvider
  {
    private readonly IHttpClientFactory httpClientFactory;

    public KSqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory)
      : base(httpClientFactory)
    {
      this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

#if NETCOREAPP3_1
      AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
#endif
    }

    public override string ContentType => "application/vnd.ksqlapi.delimited.v1";

    protected override string QueryEndPointName => "query-stream";

    protected override HttpClient OnCreateHttpClient()
    {
      var httpClient = base.OnCreateHttpClient();

      httpClient.DefaultRequestVersion = new Version(2, 0);

      return httpClient;
    }

    private RowValueJsonSerializer serializer;

    protected override RowValue<T> OnLineRead<T>(string rawJson)
    {
      //Console.WriteLine(rawJson);
      if (rawJson.StartsWith("{\"queryId\""))
      {
        var queryStreamHeader = JsonSerializer.Deserialize<QueryStreamHeader>(rawJson);

        serializer = new(queryStreamHeader);
      }      
      else if (IsErrorRow(rawJson))//{"@type":"generic_error"
      {
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawJson);

        if (errorResponse != null)
          throw new KSqlQueryException(errorResponse.Message)
          {
            ErrorCode = errorResponse.ErrorCode
          };
      }
      else
      {
        var jsonSerializerOptions = GetOrCreateJsonSerializerOptions();

        return serializer.Deserialize<T>(rawJson, jsonSerializerOptions);
      }

      return default;
    }

    protected override HttpRequestMessage CreateQueryHttpRequestMessage(HttpClient httpClient, object parameters)
    {
      var httpRequestMessage = base.CreateQueryHttpRequestMessage(httpClient, parameters);

      httpRequestMessage.Version = HttpVersion.Version20;
#if NET5_0
      httpRequestMessage.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
#endif

      return httpRequestMessage;
    }

    protected override string OnReadHeader<T>(string rawJson)
    {
      if (rawJson != null && rawJson.StartsWith("{\"queryId\""))
      {
        OnLineRead<T>(rawJson);

        var queryStreamHeader = JsonSerializer.Deserialize<QueryStreamHeader>(rawJson);

        return queryStreamHeader?.QueryId;
      }

      return null;
    }
  }
}
#endif