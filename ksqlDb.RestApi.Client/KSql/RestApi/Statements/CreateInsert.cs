using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Inserts;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal sealed class CreateInsert : CreateEntityStatement
{
  internal string Generate<T>(T entity, InsertProperties insertProperties = null)
  {
    return Generate(new InsertValues<T>(entity), insertProperties);
  }

  internal string Generate<T>(InsertValues<T> insertValues, InsertProperties insertProperties = null)
  {
    if (insertValues == null) throw new ArgumentNullException(nameof(insertValues));

    insertProperties ??= new InsertProperties();

    var entityName = GetEntityName<T>(insertProperties);

    bool isFirst = true;

    var columnsStringBuilder = new StringBuilder();
    var valuesStringBuilder = new StringBuilder();

    var useEntityType = insertProperties is {UseInstanceType: true};
    var entityType = useEntityType ? insertValues.Entity.GetType() : typeof(T);

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

      columnsStringBuilder.Append(memberInfo.Format(insertProperties.IdentifierEscaping));

      var type = GetMemberType(memberInfo);

      var value = GetValue(insertValues, insertProperties, memberInfo, type, memberInfo => IdentifierUtil.Format(memberInfo, insertProperties.IdentifierEscaping));

      valuesStringBuilder.Append(value);
    }

    string insert =
      $"INSERT INTO {entityName} ({columnsStringBuilder}) VALUES ({valuesStringBuilder});";

    return insert;
  }

  private static object GetValue<T>(InsertValues<T> insertValues, InsertProperties insertProperties,
    MemberInfo memberInfo, Type type, Func<MemberInfo, string> formatter)
  {
    var hasValue = insertValues.PropertyValues.ContainsKey(memberInfo.Format(insertProperties.IdentifierEscaping));

    object value;
    
    if (hasValue)
      value = insertValues.PropertyValues[memberInfo.Format(insertProperties.IdentifierEscaping)];
    else
      value = new CreateKSqlValue().ExtractValue(insertValues.Entity, insertProperties, memberInfo, type, formatter);

    return value;
  }
}
