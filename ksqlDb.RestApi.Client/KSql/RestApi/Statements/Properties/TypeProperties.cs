using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
  public class TypeProperties : IEntityCreationProperties
  {
    public string EntityName { get; init;  }
    public bool ShouldPluralizeEntityName => false;
    public IdentifierEscaping IdentifierEscaping { get; init; } = IdentifierEscaping.Never;
  }
}
