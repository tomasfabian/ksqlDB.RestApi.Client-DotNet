using System.Text.Json;

namespace ksqlDb.RestApi.Client.KSql.Query.Context.Options
{
  internal static class KSqlDbJsonSerializerOptions
  {
    public static JsonSerializerOptions CreateInstance() => new()
                                                            {
                                                              PropertyNameCaseInsensitive = true
                                                            };
  }
}