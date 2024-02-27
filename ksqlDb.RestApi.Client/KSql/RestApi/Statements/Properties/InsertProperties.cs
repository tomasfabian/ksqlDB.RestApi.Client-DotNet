using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

public record InsertProperties : IEntityCreationProperties, IValueFormatters
{
  public string EntityName { get; set; }

  public bool UseInstanceType { get; set; }

  public bool ShouldPluralizeEntityName { get; set; } = true;

  public IdentifierFormat IdentifierFormat { get; init; } = IdentifierFormat.None;

  public bool IncludeReadOnlyProperties { get; set; } = false;

  public Func<decimal, string> FormatDecimalValue { get; set; }

  public Func<double, string> FormatDoubleValue { get; set; }
}
