using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Kafka.DotNet.ksqlDB.Infrastructure.Extensions;
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

        var value = ExtractValue(entity, insertProperties, memberInfo, type);

        valuesStringBuilder.Append(value);
      }

      string insert =
        $"INSERT INTO {entityName} ({columnsStringBuilder}) VALUES ({valuesStringBuilder});";
			
      return insert;
    }

    private static object? ExtractValue<T>(T inputValue, InsertProperties insertProperties, MemberInfo memberInfo, Type type)
    {
      Type valueType = inputValue.GetType();
      var value = valueType.IsPrimitive || valueType == typeof(string) ? inputValue : valueType.GetProperty(memberInfo.Name)?.GetValue(inputValue);

      if (type == typeof(decimal) && insertProperties.FormatDecimalValue != null)
      {
        Debug.Assert(value != null, nameof(value) + " != null");

        value = insertProperties.FormatDecimalValue((decimal) value);
      }

      else if (type == typeof(double) && insertProperties.FormatDoubleValue != null)
      {
        Debug.Assert(value != null, nameof(value) + " != null");

        value = insertProperties.FormatDoubleValue((double) value);
      }
      else if (type == typeof(string))
        value = $"'{value}'";
      else if (type.IsPrimitive)
        ;
      else
      {
        var enumerableType = type.GetEnumerableTypeDefinition();

        if (enumerableType == null)
          return value;

        type = enumerableType.First();
        type = type.GetGenericArguments()[0];

        var source = value as IEnumerable<object>;
        source = ((IEnumerable)value).Cast<object>();
        var array = source.Select(c => ExtractValue(c, insertProperties, null, type)).ToArray();

        var sb = new StringBuilder();
        sb.Append("ARRAY[");
#if NETSTANDARD
        value = string.Join(",", array);
#else
        value = string.Join(',', array);
#endif
        sb.Append(value);
        sb.Append("]");
        value = sb.ToString();
      }

      return value;
    }
  }
}