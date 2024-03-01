using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

public interface IEntityCreationProperties
{
  public string EntityName { get; }
  public bool ShouldPluralizeEntityName { get; }
  public IdentifierEscaping IdentifierEscaping { get; }
}
