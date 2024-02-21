using System.Text.Json;
using System.Text.Json.Serialization;
using ksqlDB.RestApi.Client.KSql.Query.Context.JsonConverters;

namespace ksqlDb.RestApi.Client.KSql.Query.Context.Options;

internal static class KSqlDbJsonSerializerOptions
{
  public static JsonSerializerOptions CreateInstance()
  {
    JsonSerializerOptions jsonSerializerOptions = new()
    {
      PropertyNameCaseInsensitive = true
    };

    jsonSerializerOptions.Converters.Add(new TimeSpanToStringConverter());
    jsonSerializerOptions.Converters.Add(new JsonConverterGuid());
    jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    return jsonSerializerOptions;
  }
}
