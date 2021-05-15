using System;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties
{
  public record InsertProperties : IEntityCreationProperties
  {
    public string EntityName { get; set;}

    public bool ShouldPluralizeEntityName { get; set; } = true;

    public Func<decimal, string> FormatDecimalValue { get; set; }

    public Func<double, string> FormatDoubleValue { get; set; }
  }
}