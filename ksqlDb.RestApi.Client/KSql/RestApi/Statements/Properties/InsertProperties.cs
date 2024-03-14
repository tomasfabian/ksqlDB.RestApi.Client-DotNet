namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

/// <summary>
/// Represents a configuration for insert statements.
/// </summary>
public record InsertProperties : EntityProperties, IValueFormatters
{
  /// <summary>
  /// Determines whether to use the instance type for inferring the entity name instead of utilizing typeof(T).
  /// </summary>
  public bool UseInstanceType { get; set; }

  /// <summary>
  /// Include read-only properties during insert statement generation.
  /// </summary>
  public bool IncludeReadOnlyProperties { get; set; } = false;

  public Func<decimal, string>? FormatDecimalValue { get; set; }

  public Func<double, string>? FormatDoubleValue { get; set; }
}
