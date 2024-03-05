using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties
{
  public class TypeProperties<T> : IEntityCreationProperties
  {
    private readonly string entityName = typeof(T).ExtractTypeName().ToUpper();

    /// <summary>
    /// Is initialized to the type parameter name. Cannot be initialized with a null or empty value.
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

    /// <summary>
    /// Gets a value indicating whether the entity name should be pluralized.
    /// </summary>
    public bool ShouldPluralizeEntityName => false;

    /// <summary>
    /// Gets or sets the identifier escaping type.
    /// As ksqlDB automatically converts all identifiers to uppercase by default, it's crucial to enclose them within backticks to maintain the desired casing.
    /// </summary>
    public IdentifierEscaping IdentifierEscaping { get; init; } = IdentifierEscaping.Never;
  }
}
