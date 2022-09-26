using System.Text;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal sealed class CreateInsert : CreateEntityStatement 
{
  internal string Generate<T>(T entity, InsertProperties insertProperties = null)
  {
    insertProperties ??= new InsertProperties();
		
    var entityName = GetEntityName<T>(insertProperties);

    bool isFirst = true;

    var columnsStringBuilder = new StringBuilder();
    var valuesStringBuilder = new StringBuilder();

    var entityType = entity.GetType();

    foreach (var memberInfo in Members(entityType, insertProperties?.IncludeReadOnlyProperties))
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

      columnsStringBuilder.Append(memberInfo.Name);

      var type = GetMemberType(memberInfo);

      var value = new CreateKSqlValue().ExtractValue(entity, insertProperties, memberInfo, type);

      valuesStringBuilder.Append(value);
    }

    string insert =
      $"INSERT INTO {entityName} ({columnsStringBuilder}) VALUES ({valuesStringBuilder});";
			
    return insert;
  }
}