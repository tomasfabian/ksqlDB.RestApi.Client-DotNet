#if !NETSTANDARD
using System.Net;
using System.Text.Json;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;
using Microsoft.Extensions.Logging;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

#nullable disable
namespace ksqlDB.RestApi.Client.KSql.RestApi
{
  internal class KSqlDbQueryStreamProvider : KSqlDbProvider
  {
    public KSqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory, IMetadataProvider metadataProvider, KSqlDbProviderOptions options, ILogger logger = null)
      : base(httpClientFactory, metadataProvider, options, logger)
    {
#if NETCOREAPP3_1
      AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
#endif
    }

    public override string ContentType => "application/vnd.ksqlapi.delimited.v1";

    protected override string QueryEndPointName => "query-stream";

    private RowValueJsonSerializer serializer;

    protected override RowValue<T> OnLineRead<T>(string rawJson)
    {
      if (rawJson.StartsWith("{\"queryId\""))
      {
        var queryStreamHeader = JsonSerializer.Deserialize<QueryStreamHeader>(rawJson);

        serializer = new(queryStreamHeader);
      }      
      else if (IsErrorRow(rawJson))//{"@type":"generic_error"
      {
        OnError<T>(rawJson);
      }
      else
      {
        var jsonSerializerOptions = GetOrCreateJsonSerializerOptions();

        return serializer.Deserialize<T>(rawJson, jsonSerializerOptions);
      }

      return default;
    }

    private static void OnError<T>(string rawJson)
    {
      var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawJson);

      if (errorResponse != null)
        throw new KSqlQueryException(errorResponse.Message)
              {
                ErrorCode = errorResponse.ErrorCode
              };
    }

    protected override HttpRequestMessage CreateQueryHttpRequestMessage(HttpClient httpClient, object parameters)
    {
      var httpRequestMessage = base.CreateQueryHttpRequestMessage(httpClient, parameters);

      httpRequestMessage.Version = HttpVersion.Version20;
#if NET5_0_OR_GREATER 
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
      
      if (IsErrorRow(rawJson))//{"@type":"generic_error"
      {
        OnError<T>(rawJson);
      }

      return null;
    }
  }
}
#endif
