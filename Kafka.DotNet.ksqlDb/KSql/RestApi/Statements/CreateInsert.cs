using System;
using System.Collections;
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

    private object? ExtractValue<T>(T inputValue, InsertProperties insertProperties, MemberInfo memberInfo, Type type)
    {
      Type valueType = inputValue.GetType();
      var value = valueType.IsPrimitive || valueType == typeof(string) ? inputValue : valueType.GetProperty(memberInfo.Name)?.GetValue(inputValue);

      if (value == null)
        return "NULL";

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
      else if (type.IsArray)
      {
        var source = ((IEnumerable)value).Cast<object>();
        var array = source.Select(c => ExtractValue(c, insertProperties, null, type.GetElementType())).ToArray();
        value = PrintArray(array);
      }
      else if (!type.IsGenericType && (type.IsClass || type.IsStruct()))
      {
        GenerateStruct<T>(insertProperties, type, ref value);
      }
      else
      {
        value = GenerateEnumerableValue(type, value, insertProperties);
      }

      return value;
    }

    private void GenerateStruct<T>(InsertProperties insertProperties, Type type, ref object value)
    {
      bool isFirst = true;

      var sb = new StringBuilder();
      sb.Append("STRUCT(");
      foreach (var memberInfo2 in Members(type))
      {
        if (isFirst)
          isFirst = false;
        else
          sb.Append(", ");

        type = GetMemberType<T>(memberInfo2);

        var innerValue = ExtractValue(value, insertProperties, memberInfo2, type);
        sb.Append($"{memberInfo2.Name} := {innerValue}");
      }

      sb.Append(")");

      value = sb.ToString();
    }

    private object GenerateEnumerableValue(Type type, object value, InsertProperties insertProperties)
    {
      var enumerableType = type.GetEnumerableTypeDefinition();

      if (enumerableType == null)
        return value;

      type = enumerableType.First();
      type = type.GetGenericArguments()[0];
        
      var source = ((IEnumerable)value).Cast<object>();
      var array = source.Select(c => ExtractValue(c, insertProperties, null, type)).ToArray();

      return PrintArray(array);
    }

    private static object PrintArray(object[] array)
    {
      var sb = new StringBuilder();
      sb.Append("ARRAY[");
#if NETSTANDARD
      var value = string.Join(",", array);
#else
      var value = string.Join(',', array);
#endif
      sb.Append(value);
      sb.Append("]");
      
      value = sb.ToString();
      
      return value;
    }
  }
}