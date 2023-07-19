namespace ksqlDB.RestApi.Client.KSql.Query.Functions;

/// <summary>
/// Represents an entry with a key and a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public struct Entry<TValue>
{
  /// <summary>
  /// Gets or sets the key of the entry.
  /// </summary>
  public string K { get; set; }

  /// <summary>
  /// Gets or sets the value of the entry.
  /// </summary>
  public TValue V { get; set; }
}
