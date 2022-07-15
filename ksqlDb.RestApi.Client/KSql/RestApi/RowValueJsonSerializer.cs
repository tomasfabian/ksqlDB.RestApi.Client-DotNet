using System;
using System.Text.Json;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

internal class RowValueJsonSerializer
{
  private readonly QueryStreamHeader queryStreamHeader;

  internal RowValueJsonSerializer(QueryStreamHeader queryStreamHeader)
  {
    this.queryStreamHeader = queryStreamHeader ?? throw new ArgumentNullException(nameof(queryStreamHeader));
  }

  internal RowValue<T> Deserialize<T>(string rawJson, JsonSerializerOptions jsonSerializerOptions)
  {
    var result = rawJson.Substring(1, rawJson.Length - 2);

    if (queryStreamHeader.ColumnTypes.Length == 1 && !typeof(T).IsAnonymousType())
      return new RowValue<T>(JsonSerializer.Deserialize<T>(result, jsonSerializerOptions));

    var jsonRecord = new JsonArrayParser().CreateJson(queryStreamHeader.ColumnNames, result);

    var record = JsonSerializer.Deserialize<T>(jsonRecord, jsonSerializerOptions);

    return new RowValue<T>(record);
  }
}