#if !NETSTANDARD
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;
using Microsoft.Extensions.Logging;
using IHttpClientFactory = ksqlDB.RestApi.Client.KSql.RestApi.Http.IHttpClientFactory;
using JsonTypeInfoResolver = ksqlDb.RestApi.Client.KSql.RestApi.Json.JsonTypeInfoResolver;

#nullable disable
namespace ksqlDB.RestApi.Client.KSql.RestApi
{
  internal class KSqlDbQueryStreamProvider : KSqlDbProvider
  {
    private readonly IMetadataProvider metadataProvider;

    public KSqlDbQueryStreamProvider(IHttpClientFactory httpClientFactory, IMetadataProvider metadataProvider, KSqlDbProviderOptions options, ILogger logger = null)
      : base(httpClientFactory, options, logger)
    {
      this.metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
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

    protected override JsonSerializerOptions OnCreateJsonSerializerOptions()
    {
      var jsonSerializerOptions = base.OnCreateJsonSerializerOptions();

      if (jsonSerializerOptions.TypeInfoResolver == null)
      {
        var defaultJsonTypeInfoResolver = new DefaultJsonTypeInfoResolver();
        var resolver = new JsonTypeInfoResolver(defaultJsonTypeInfoResolver)
        {
          Modifiers = { JsonPropertyNameModifier }
        };
        jsonSerializerOptions.TypeInfoResolver = resolver;
      }
      else if(jsonSerializerOptions.TypeInfoResolver is not JsonTypeInfoResolver)
      {
        var resolver = new JsonTypeInfoResolver(jsonSerializerOptions.TypeInfoResolver)
        {
          Modifiers = { JsonPropertyNameModifier }
        };

        jsonSerializerOptions.TypeInfoResolver = resolver;
      }

      return jsonSerializerOptions;
    }

    internal void JsonPropertyNameModifier(JsonTypeInfo jsonTypeInfo)
    {
      JsonPropertyNameModifier(jsonTypeInfo, metadataProvider);
    }

    internal static void JsonPropertyNameModifier(JsonTypeInfo jsonTypeInfo, IMetadataProvider metadataProvider)
    {
      var entityMetadata = metadataProvider.GetEntities().FirstOrDefault(c => c.Type == jsonTypeInfo.Type);

      foreach (var typeInfoProperty in jsonTypeInfo.Properties)
      {
        var fieldMetadata =
          entityMetadata?.FieldsMetadata?.FirstOrDefault(c => c.MemberInfo.Name == typeInfoProperty.Name);

        if (fieldMetadata != null && !string.IsNullOrEmpty(fieldMetadata.ColumnName))
          typeInfoProperty.Name = fieldMetadata.ColumnName;
      }
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
