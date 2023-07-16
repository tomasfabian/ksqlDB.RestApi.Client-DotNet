namespace ksqlDB.RestApi.Client.KSql.Query.Options;

/// <summary>
/// Provides extension methods for converting string values to <see cref="AutoOffsetReset"/> and vice versa.
/// </summary>
public static class AutoOffsetResetExtensions
{
  /// <summary>
  /// Converts a string value to <see cref="AutoOffsetReset"/>.
  /// </summary>
  /// <param name="autoOffsetResetValue">The string value representing the auto offset reset policy.</param>
  /// <returns>The corresponding <see cref="AutoOffsetReset"/> value.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided <paramref name="autoOffsetResetValue"/> is not a valid option.</exception>
  public static AutoOffsetReset ToAutoOffsetReset(this string autoOffsetResetValue)
  {        
    if (autoOffsetResetValue == "earliest")
      return AutoOffsetReset.Earliest;
        
    if (autoOffsetResetValue == "latest")
      return AutoOffsetReset.Latest;

    throw new ArgumentOutOfRangeException(nameof(autoOffsetResetValue), autoOffsetResetValue, null);
  }

  /// <summary>
  /// Converts an <see cref="AutoOffsetReset"/> value to its corresponding string representation for KSql.
  /// </summary>
  /// <param name="value">The <see cref="AutoOffsetReset"/> value.</param>
  /// <returns>The string representation of the <see cref="AutoOffsetReset"/> value for KSql.</returns>
  public static string ToKSqlValue(this AutoOffsetReset value)
  {
    return value.ToString().ToLower();
  }
}
