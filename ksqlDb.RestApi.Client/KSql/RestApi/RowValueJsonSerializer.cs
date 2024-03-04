using System.Text.Json;
using System.Text.RegularExpressions;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;

namespace ksqlDB.RestApi.Client.KSql.RestApi;

internal class RowValueJsonSerializer
{
  private readonly QueryStreamHeader queryStreamHeader;
  private readonly bool isSingleAnonymousColumn;
  private readonly bool isMapColumn;

  internal RowValueJsonSerializer(QueryStreamHeader queryStreamHeader)
  {
    this.queryStreamHeader = queryStreamHeader ?? throw new ArgumentNullException(nameof(queryStreamHeader));

    if (this.queryStreamHeader.ColumnNames.Length != queryStreamHeader.ColumnTypes.Length)
      throw new InvalidOperationException("Length of the column names differs from column types");

    if (queryStreamHeader.ColumnTypes.Length == 1)
    {
      isSingleAnonymousColumn = Regex.Matches(queryStreamHeader.ColumnNames[0], anonymousColumnRegex).Count > 0;
      isMapColumn = Regex.Matches(queryStreamHeader.ColumnTypes[0], structRegex).Count > 0;
    }
  }

  private readonly string anonymousColumnRegex = "^KSQL_COL_\\d+";
  private readonly string structRegex = "^MAP<";

  internal RowValue<T> Deserialize<T>(string rawJson, JsonSerializerOptions jsonSerializerOptions)
  {
#if NETSTANDARD
    var result = rawJson.Substring(1, rawJson.Length - 2);
#else
    var result = rawJson[1 .. ^1];
#endif

    if (queryStreamHeader.ColumnTypes.Length == 1 && !typeof(T).IsAnonymousType())
    {
      var type = typeof(T);
      var isAllowedType = type.IsPrimitive || type.IsArray || type.IsEnum;

      if (isSingleAnonymousColumn || isMapColumn || isAllowedType)
        return new RowValue<T>(JsonSerializer.Deserialize<T>(result, jsonSerializerOptions));
    }

    var jsonRecord = new JsonArrayParser().CreateJson(queryStreamHeader.ColumnNames, result);

    var record = JsonSerializer.Deserialize<T>(jsonRecord, jsonSerializerOptions);

    return new RowValue<T>(record);
  }
}
