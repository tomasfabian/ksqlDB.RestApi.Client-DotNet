using System.Net;
using System.Text.Json;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;
using ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi.Responses.Query;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;

namespace ksqlDb.RestApi.Client.ProtoBuf.KSql.RestApi;

internal class KSqlDbQueryProvider : KSqlDbProvider 
{
  public KSqlDbQueryProvider(IHttpClientFactory httpClientFactory, KSqlDbProviderOptions options, ILogger? logger = null)
    : base(httpClientFactory, options, logger)
  {
#if NETCOREAPP3_1
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
#endif
  }

  public override string ContentType => "application/vnd.ksql.v1+protobuf";

  protected override string QueryEndPointName => "query";

  private string ExtractRow(string rawData)
  {
    if (string.IsNullOrEmpty(rawData))
      return rawData;

    if (rawData.StartsWith("["))
      rawData = rawData.Substring(startIndex: 1);
    if (rawData.EndsWith(","))
      rawData = rawData.Substring(0, rawData.Length - 1);
    if (rawData.EndsWith("]"))
      rawData = rawData.Substring(0, rawData.Length - 1);

    return rawData;
  }

  private HeaderResponse? headerResponse;

  protected override RowValue<T>? OnLineRead<T>(string rawJson)
  {
    if (rawJson == String.Empty)
      return default;

    rawJson = ExtractRow(rawJson);

    if (IsErrorRow(rawJson))
    {
      OnError(rawJson);
    }

    if (headerResponse == null && rawJson.StartsWith("{\"header\""))
      headerResponse = JsonSerializer.Deserialize<HeaderResponse>(rawJson, GetOrCreateJsonSerializerOptions());

    if (rawJson.StartsWith("{\"row\"", StringComparison.OrdinalIgnoreCase))
      return CreateRowValue<T>(rawJson);

    return default;
  }

  private void OnError(string rawJson)
  {
    var jsonSerializerOptions = GetOrCreateJsonSerializerOptions();

    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawJson, jsonSerializerOptions);

    if (errorResponse != null)
      throw new KSqlQueryException(errorResponse.Message)
      {
        Statement = errorResponse.StatementText,
        ErrorCode = errorResponse.ErrorCode
      };
  }

  protected override HttpRequestMessage CreateQueryHttpRequestMessage(HttpClient httpClient, object parameters)
  {
    var httpRequestMessage = base.CreateQueryHttpRequestMessage(httpClient, parameters);

#if !NETSTANDARD
    httpRequestMessage.Version = HttpVersion.Version20;
#endif

#if NET5_0_OR_GREATER
    httpRequestMessage.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
#endif

    return httpRequestMessage;
  }

  protected override string? OnReadHeader<T>(string? rawJson)
  {
    if (rawJson != null && rawJson.StartsWith("{\"queryId\""))
    {
      OnLineRead<T>(rawJson);

      var queryStreamHeader = JsonSerializer.Deserialize<QueryStreamHeader>(rawJson);

      return queryStreamHeader?.QueryId;
    }

    if (IsErrorRow(rawJson))
    {
      OnError(rawJson);
    }

    return null;
  }

  private static string Base64Decode(string base64EncodedData)
  {
    var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

    return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
  }

  private RowValue<T>? CreateRowValue<T>(string rawJson)
  {
    var jsonSerializerOptions = GetOrCreateJsonSerializerOptions();

    var protoResponse = JsonSerializer.Deserialize<RowResponse>(rawJson, jsonSerializerOptions);

    if (string.IsNullOrEmpty(protoResponse!.Row.ProtobufBytes))
      return default;

    var base64Encoded = Base64Decode(protoResponse!.Row.ProtobufBytes!);

    using var stream = GenerateStreamFromString(base64Encoded);

    var record = Serializer.Deserialize<T>(stream);

    return new RowValue<T>(record);
  }

  private static Stream GenerateStreamFromString(string rawData)
  {
    var stream = new MemoryStream();
    var writer = new StreamWriter(stream);

    writer.Write(rawData);
    writer.Flush();

    stream.Position = 0;

    return stream;
  }
}