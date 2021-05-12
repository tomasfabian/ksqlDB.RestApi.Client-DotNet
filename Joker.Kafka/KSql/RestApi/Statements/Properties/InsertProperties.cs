namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties
{
  public record InsertProperties : IEntityCreationProperties
  {
    public string EntityName { get; set;}

    public bool ShouldPluralizeEntityName { get; set; } = true;
  }
}