using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
  public class TypeProperties<T> : IEntityCreationProperties
  {
    private readonly string entityName = typeof(T).ExtractTypeName().ToUpper();

    /// <summary>
    /// Is initialized to the type parameter name. Cannot be initialize with a null or empty value.
    /// </summary>
    public string EntityName
    {
      get => entityName;
      init
      {
        if (!string.IsNullOrEmpty(value))
        {
          entityName = value;
        }
      }
    }

    public bool ShouldPluralizeEntityName => false;
    public IdentifierEscaping IdentifierEscaping { get; init; } = IdentifierEscaping.Never;
  }
}
