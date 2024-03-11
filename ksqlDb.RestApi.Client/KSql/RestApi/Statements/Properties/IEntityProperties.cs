using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
#nullable enable
  public interface IEntityProperties
  {
    /// <summary>
    /// Gets the entity name that overrides the automatically inferred name.
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Gets a value indicating whether the entity name should be pluralized.
    /// </summary>
    public bool ShouldPluralizeEntityName { get; }

    /// <summary>
    /// Gets the identifier escaping type.
    /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
    /// </summary>
    public IdentifierEscaping IdentifierEscaping { get; }
  }
}
