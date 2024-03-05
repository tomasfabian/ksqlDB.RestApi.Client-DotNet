using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

public record InsertProperties : IEntityCreationProperties, IValueFormatters
{
  /// <summary>
  /// Gets the entity name that overrides the automatically inferred name.
  /// </summary>
  public string EntityName { get; set; }

  /// <summary>
  /// Determines whether to use the instance type for inferring the entity name instead of utilizing typeof(T).
  /// </summary>
  public bool UseInstanceType { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether the entity name should be pluralized.
  /// </summary>
  public bool ShouldPluralizeEntityName { get; set; } = true;

  /// <summary>
  /// Gets or sets the identifier escaping type.
  /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
  /// </summary>
  public IdentifierEscaping IdentifierEscaping { get; init; } = IdentifierEscaping.Never;

  /// <summary>
  /// Include read-only properties during insert statement generation.
  /// </summary>
  public bool IncludeReadOnlyProperties { get; set; } = false;

  public Func<decimal, string> FormatDecimalValue { get; set; }

  public Func<double, string> FormatDoubleValue { get; set; }
}
