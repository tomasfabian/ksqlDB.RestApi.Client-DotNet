using System.Text;
using Kafka.DotNet.ksqlDB.KSql.RestApi.Statements.Properties;

namespace Kafka.DotNet.ksqlDB.KSql.RestApi.Statements
{
  internal sealed class CreateInsert : CreateEntityStatement 
  {
    internal string Generate<T>(T entity, InsertProperties insertProperties = null)
    {
      insertProperties ??= new InsertProperties();
		
      var entityName = GetEntityName<T>(insertProperties);

      bool isFirst = true;

      var columnsStringBuilder = new StringBuilder();
      var valuesStringBuilder = new StringBuilder();

      foreach (var memberInfo in Members<T>())
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

        var type = GetMemberType<T>(memberInfo);

        var value = new CreateKSqlValue().ExtractValue(entity, insertProperties, memberInfo, type);

        valuesStringBuilder.Append(value);
      }

      string insert =
        $"INSERT INTO {entityName} ({columnsStringBuilder}) VALUES ({valuesStringBuilder});";
			
      return insert;
    }
  }
}