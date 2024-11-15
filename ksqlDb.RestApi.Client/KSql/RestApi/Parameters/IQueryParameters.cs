namespace ksqlDB.RestApi.Client.KSql.RestApi.Parameters;

/// <summary>
/// Represents query parameters for a KSqlDb endpoint.
/// </summary>
public interface IQueryParameters : IQueryOptions
{
  /// <summary>
  /// A semicolon-delimited sequence of SQL statements to run.
  /// </summary>
  string Sql { get; set; }

  void Set<T>(string key, T value);

  T Get<T>(string key);
}
