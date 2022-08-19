using System.Text.Json;
using ksqlDB.RestApi.Client.KSql.RestApi.Exceptions;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Query;

internal class KSqlDbProviderValueReader
{
  internal static string ExtractRow(string rawData)
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
  
  internal static void OnError(string rawJson, JsonSerializerOptions jsonSerializerOptions)
  {
    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(rawJson, jsonSerializerOptions);

    if (errorResponse != null)
      throw new KSqlQueryException(errorResponse.Message)
      {
        Statement = errorResponse.StatementText,
        ErrorCode = errorResponse.ErrorCode
      };
  }

  internal static bool IsErrorRow(string rawJson)
  {
    return rawJson.StartsWith("{\"@type\":\"statement_error\"") || rawJson.StartsWith("{\"@type\":\"generic_error\"");
  }
}