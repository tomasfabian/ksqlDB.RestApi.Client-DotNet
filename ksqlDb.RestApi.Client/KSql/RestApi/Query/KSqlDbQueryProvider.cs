using System.Text.Json;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.KSql.Query.Context.Options;
using ksqlDB.RestApi.Client.KSql.RestApi.Http;
using ksqlDB.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Query;
using Microsoft.Extensions.Logging;
using ksqlDb.RestApi.Client.FluentAPI.Builders;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Query;

#nullable disable
internal class KSqlDbQueryProvider : KSqlDbProvider
{
  public KSqlDbQueryProvider(IHttpV1ClientFactory httpClientFactory, IMetadataProvider metadataProvider, KSqlDbProviderOptions options, ILogger logger = null)
    : base(httpClientFactory, metadataProvider, options, logger)
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

    rawJson = KSqlDbProviderValueReader.ExtractRow(rawJson);

    if (IsErrorRow(rawJson))
    {
      KSqlDbProviderValueReader.OnError(rawJson, GetOrCreateJsonSerializerOptions());
    }

    if (headerResponse == null && rawJson.StartsWith("{\"header\""))
      headerResponse = JsonSerializer.Deserialize<HeaderResponse>(rawJson);

    if (rawJson.StartsWith("{\"row\""))
      return CreateRowValue<T>(rawJson);

    return default;
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
      KSqlDbProviderValueReader.OnError(rawJson, GetOrCreateJsonSerializerOptions());

    return null;
  }
}
