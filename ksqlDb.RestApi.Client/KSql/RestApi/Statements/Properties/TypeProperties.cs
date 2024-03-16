namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
  /// <summary>
  /// Represents a configuration for a ksqlDB type.
  /// </summary>
  public record TypeProperties : EntityProperties
  {
    public TypeProperties()
    {
      ShouldPluralizeEntityName = false;
    }
  }
}
