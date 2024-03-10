namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
  /// <summary>
  /// Represents a configuration for dropping a ksqlDB stream or table.
  /// </summary>
  public record DropFromItemProperties : EntityProperties
  {
    /// <summary>
    /// Gets or sets a value indicating whether to use the "IF EXISTS" clause when dropping the entity.
    /// </summary>
    public bool UseIfExistsClause { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to delete the associated topic when dropping the entity.
    /// </summary>
    public bool DeleteTopic { get; init; }
  }
}
