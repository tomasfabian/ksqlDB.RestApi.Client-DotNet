using System.Text.Json;

namespace ksqlDb.RestApi.Client.KSql.Query.Context.Options
{
  public abstract class KSqlDbProviderOptions
  {
    internal JsonSerializerOptions JsonSerializerOptions { get; set; } = KSqlDbJsonSerializerOptions.CreateInstance();

    public bool DisposeHttpClient { get; set; }
  }
}