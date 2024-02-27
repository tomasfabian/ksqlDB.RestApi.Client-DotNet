namespace ksqlDB.RestApi.Client.KSql.RestApi.Enums
{
  /// <summary>
  /// Formats for column identifiers
  /// </summary>
  public enum IdentifierFormat
  {
    /// <summary>
    ///  No identifier is formatted
    /// </summary>
    None = 0,
    /// <summary>
    /// Identifiers that are keywords are formatted
    /// </summary>
    Keywords = 1,
    /// <summary>
    /// All identifiers are formatted
    /// </summary>
    Always = 2,
  }
}
