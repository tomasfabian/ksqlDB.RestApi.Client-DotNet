using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using Pluralize.NET;

namespace ksqlDb.RestApi.Client.KSql.RestApi.Statements.Providers
{
  internal sealed class EntityProvider
  {
    private readonly Pluralizer englishPluralizationService = new();

    internal string GetFormattedName<T>(IEntityProperties entityProperties, Func<string, IdentifierEscaping, string> formatter = null)
    {
      return GetFormattedName(typeof(T), entityProperties, formatter);
    }

    internal string GetFormattedName(Type type, IEntityProperties entityProperties, Func<string, IdentifierEscaping, string> formatter = null)
    {
      string entityName = entityProperties.EntityName;

      if (string.IsNullOrEmpty(entityName))
        entityName = type.ExtractTypeName();

      if (entityProperties is { ShouldPluralizeEntityName: true })
        entityName = englishPluralizationService.Pluralize(entityName);

      return formatter != null
        ? formatter(entityName, entityProperties.IdentifierEscaping)
        : IdentifierUtil.Format(entityName, entityProperties.IdentifierEscaping);
    }
  }
}
