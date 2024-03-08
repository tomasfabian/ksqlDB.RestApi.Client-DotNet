using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
  /// <summary>
  /// Represents a configuration for a ksqlDB entity.
  /// </summary>
  public record EntityProperties : IEntityProperties
  {
    /// <summary>
    /// Gets or sets the entity name that overrides the automatically inferred name.
    /// </summary>
    public string EntityName { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity name should be pluralized.
    /// </summary>
    public bool ShouldPluralizeEntityName { get; init; } = true;

    /// <summary>
    /// Gets or sets the identifier escaping type.
    /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
    /// </summary>
    public IdentifierEscaping IdentifierEscaping { get; init; } = IdentifierEscaping.Never;
  }
}
