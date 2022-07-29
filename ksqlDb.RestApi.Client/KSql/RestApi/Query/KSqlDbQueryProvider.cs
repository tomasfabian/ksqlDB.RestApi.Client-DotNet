using System;
using System.Linq;
using System.Text.Json;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query;
using Microsoft.Extensions.Logging;
using ErrorResponse = ksqlDB.RestApi.Client.KSql.RestApi.Responses.ErrorResponse;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Query;

internal class KSqlDbQueryProvider : KSqlDbProvider
{
  public KSqlDbQueryProvider(IHttpV1ClientFactory httpClientFactory, KSqlDbProviderOptions options, ILogger logger = null)
    : base(httpClientFactory, options, logger)
  {
  }

  public override string ContentType => "application/vnd.ksql.v1+json";

  protected override string QueryEndPointName => "query";

  private HeaderResponse headerResponse;
  private string[] headerColumns;

  protected override RowValue<T> OnLineRead<T>(string rawJson)
  {
    if (rawJson == String.Empty)
      return default;

    rawJson = ExtractRow(rawJson);

    if (IsErrorRow(rawJson))
    {
      OnError(rawJson);
    }

    if (headerResponse == null && rawJson.StartsWith("{\"header\""))
      headerResponse = JsonSerializer.Deserialize<HeaderResponse>(rawJson);

    if (rawJson.StartsWith("{\"row\""))
      return CreateRowValue<T>(rawJson);

    return default;
  }

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

  private static void OnError(string rawJson)
  {
    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawJson);

    if (errorResponse != null)
      throw new KSqlQueryException(errorResponse.Message)
      {
        Statement = errorResponse.StatementText,
        ErrorCode = errorResponse.ErrorCode
      };
  }

  private RowValue<T> CreateRowValue<T>(string rawJson)
  {
    if (headerColumns == null)
    {
      var schema = headerResponse.Header.Schema;
      headerColumns = new HeaderColumnExtractor().GetColumnsFromSchema(schema).ToArray();
    }

    var jsonSerializerOptions = GetOrCreateJsonSerializerOptions();

    var columnValues = HeaderColumnExtractor.ExtractColumnValues(rawJson);

    if (headerColumns.Length == 1 && !typeof(T).IsAnonymousType())
      return new RowValue<T>(JsonSerializer.Deserialize<T>(columnValues, jsonSerializerOptions));

    var jsonRecord = new JsonArrayParser().CreateJson(headerColumns, columnValues);
    var record = JsonSerializer.Deserialize<T>(jsonRecord, jsonSerializerOptions);

    return new RowValue<T>(record);
  }

  protected override string OnReadHeader<T>(string rawJson)
  {
    if (rawJson != null && rawJson.StartsWith("[{\"header\""))
    {
      OnLineRead<T>(rawJson);

      return headerResponse?.Header?.QueryId;
    }
    
    if (IsErrorRow(rawJson))
      OnError(rawJson);

    return null;
  }
}