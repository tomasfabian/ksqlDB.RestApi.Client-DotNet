using System.Text.Json;
using ksqlDB.RestApi.Client.KSql.Query.Context.JsonConverters;

namespace ksqlDb.RestApi.Client.KSql.Query.Context.Options
{
  internal static class KSqlDbJsonSerializerOptions
  {
    public static JsonSerializerOptions CreateInstance()
    {
      JsonSerializerOptions jsonSerializerOptions = new()
      {
        PropertyNameCaseInsensitive = true
      };

      jsonSerializerOptions.Converters.Add(new TimeSpanToStringConverter());

      return jsonSerializerOptions;
    }
  }
}