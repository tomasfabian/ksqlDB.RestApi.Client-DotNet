using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using ksqlDb.RestApi.Client.Metadata;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal sealed class CreateInsert : EntityInfo
{
  private readonly IMetadataProvider metadataProvider;

  public CreateInsert(IMetadataProvider metadataProvider)
    : base(metadataProvider)
  {
    this.metadataProvider = metadataProvider;
  }

  internal string Generate<T>(T entity, InsertProperties insertProperties)
  {
    return Generate(new InsertValues<T>(entity), insertProperties);
  }

  internal string Generate<T>(InsertValues<T> insertValues, InsertProperties insertProperties)
  {
    if (insertProperties.ShouldPluralizeEntityName == null)
      insertProperties = insertProperties with { ShouldPluralizeEntityName = true };

    var entityName = EntityProvider.GetFormattedName<T>(insertProperties);

    var columnsStringBuilder = new StringBuilder();
    var valuesStringBuilder = new StringBuilder();

    var useInstanceType = insertProperties is {UseInstanceType: true};
    var entityType = useInstanceType && insertValues.Entity != null ? insertValues.Entity.GetType() : typeof(T);

    bool isFirst = true;

    foreach (var memberInfo in Members(entityType, insertProperties.IncludeReadOnlyProperties))
    {
      if (isFirst)
      {
        isFirst = false;
      }
      else
      {
        columnsStringBuilder.Append(", ");
        valuesStringBuilder.Append(", ");
      }

      columnsStringBuilder.Append(memberInfo.Format(insertProperties.IdentifierEscaping, metadataProvider));

      var type = GetMemberType(memberInfo);

      var value = GetValue(insertValues, insertProperties, memberInfo, type, mi => IdentifierUtil.Format(mi, insertProperties.IdentifierEscaping, metadataProvider));

      valuesStringBuilder.Append(value);
    }

    string insert =
      $"INSERT INTO {entityName} ({columnsStringBuilder}) VALUES ({valuesStringBuilder});";

    return insert;
  }

  private object GetValue<T>(InsertValues<T> insertValues, InsertProperties insertProperties,
    MemberInfo memberInfo, Type type, Func<MemberInfo, string> formatter)
  {
    var hasValue = insertValues.PropertyValues.ContainsKey(memberInfo.Format(insertProperties.IdentifierEscaping, metadataProvider));

    object value;

    if (hasValue)
      value = insertValues.PropertyValues[memberInfo.Format(insertProperties.IdentifierEscaping, metadataProvider)];
    else
      value = new CreateKSqlValue(metadataProvider).ExtractValue(insertValues.Entity, insertProperties, memberInfo, type, formatter);

    return value;
  }

  protected override bool IncludeMemberInfo(Type type, EntityMetadata? entityMetadata, MemberInfo memberInfo, bool? includeReadOnly = null)
  {
    var fieldMetadata = entityMetadata?.GetFieldMetadataBy(memberInfo);
    if (fieldMetadata is {IgnoreInDML: true})
      return false;

    return base.IncludeMemberInfo(type, entityMetadata, memberInfo) && !memberInfo.GetCustomAttributes().OfType<IgnoreByInsertsAttribute>().Any();
  }
}
