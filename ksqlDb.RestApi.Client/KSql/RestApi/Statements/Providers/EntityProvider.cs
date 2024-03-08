using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Pluralize.NET;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers
{
  internal class EntityProvider
  {
    private readonly Pluralizer englishPluralizationService = new();

    internal string GetName<T>(IEntityProperties entityProperties)
    {
      string entityName = entityProperties.EntityName;

      if (string.IsNullOrEmpty(entityName))
        entityName = typeof(T).Name;

      if (entityProperties is { ShouldPluralizeEntityName: true })
        entityName = englishPluralizationService.Pluralize(entityName);

      return IdentifierUtil.Format(entityName, entityProperties.IdentifierEscaping);
    }
  }
}
