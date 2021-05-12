using System;
using System.Linq;
using System.Text.Json;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Exceptions;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Parsers;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.Query;
using ErrorResponse = Kafka.DotNet.ksqlDB.KSql.RestApi.Responses.ErrorResponse;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Query
{
  internal class KSqlDbQueryProvider : KSqlDbProvider
  {
    public KSqlDbQueryProvider(IHttpClientFactory httpClientFactory)
      : base(httpClientFactory)
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

      //Console.WriteLine(rawJson);

      if (rawJson.StartsWith("["))
        rawJson = rawJson.Substring(startIndex: 1);
      if (rawJson.EndsWith(","))
        rawJson = rawJson.Substring(0, rawJson.Length - 1);
      if (rawJson.EndsWith("]"))
        rawJson = rawJson.Substring(0, rawJson.Length - 1);

      if (rawJson.Contains("statement_error"))
      {
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawJson);

        if (errorResponse != null)
          throw new KSqlQueryException(errorResponse.Message)
          {
            Statement = errorResponse.StatementText,
            ErrorCode = errorResponse.ErrorCode
          };
      }

      if (rawJson.Contains("header"))
        headerResponse = JsonSerializer.Deserialize<HeaderResponse>(rawJson);

      if (rawJson.Contains("row"))
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
  }
}