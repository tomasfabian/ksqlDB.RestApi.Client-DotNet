namespace ksqlDB.RestApi.Client.KSql.Query.Context
{
  /// <summary>
  /// Options class for ksqlDB REST API client configuration.
  /// </summary>
  public sealed class KSqlDBRestApiClientOptions
  {
    /// <summary>
    /// Gets or sets a value indicating whether table or stream name should be pluralized from the item name (by default: true).
    /// </summary>
    public bool ShouldPluralizeFromItemName { get; set; } = true;
  }
}
