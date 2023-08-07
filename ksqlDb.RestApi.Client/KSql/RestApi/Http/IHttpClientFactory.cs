namespace ksqlDB.RestApi.Client.KSql.RestApi.Http;

/// <summary>
/// Defines a factory for creating instances of <see cref="HttpClient"/>.
/// </summary>
public interface IHttpClientFactory
{
  /// <summary>
  /// Creates a new instance of <see cref="HttpClient"/>.
  /// </summary>
  /// <returns>A new instance of <see cref="HttpClient"/>.</returns>

  HttpClient CreateClient();
}
