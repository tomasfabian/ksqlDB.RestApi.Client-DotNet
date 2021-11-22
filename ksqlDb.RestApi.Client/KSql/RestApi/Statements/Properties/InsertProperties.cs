using System;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
  public record InsertProperties : IEntityCreationProperties, IValueFormatters
  {
    public string EntityName { get; set;}

    public bool ShouldPluralizeEntityName { get; set; } = true;

    public bool IncludeReadOnlyProperties { get; set; } = false;

    public Func<decimal, string> FormatDecimalValue { get; set; }

    public Func<double, string> FormatDoubleValue { get; set; }
  }
}