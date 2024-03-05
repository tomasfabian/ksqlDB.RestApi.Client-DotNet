namespace ksqlDB.RestApi.Client.KSql.RestApi.Enums
{
  /// <summary>
  /// Formats for user-defined identifiers e.g. streams, tables, columns, and other objects.
  /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
  /// </summary>
  public enum IdentifierEscaping
  {
    /// <summary>
    /// No identifier is formatted
    /// </summary>
    Never = 0,
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
