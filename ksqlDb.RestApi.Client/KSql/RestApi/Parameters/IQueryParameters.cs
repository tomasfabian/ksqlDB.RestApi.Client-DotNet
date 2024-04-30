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

  /// <summary>
  /// Indexer to access properties by key.
  /// </summary>
  /// <param name="key">The key of the property.</param>
  /// <returns>The value of the property.</returns>
  string this[string key] { get; set; }
}
