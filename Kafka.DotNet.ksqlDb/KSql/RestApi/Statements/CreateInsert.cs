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


      bool useValue = valueType.IsPrimitive || valueType == typeof(string)  || valueType.IsStruct() || typeof(IEnumerable).IsAssignableFrom(valueType);

      if (memberInfo?.MemberType == MemberTypes.Property)
      {
        foreach (MethodInfo am in ((PropertyInfo)memberInfo).GetAccessors())
        {
          useValue = false;
        }
      }

      var value = useValue ? inputValue : valueType.GetProperty(memberInfo.Name)?.GetValue(inputValue);

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
        value = value.ToString();
      else if (type.IsDictionary())
        GenerateSMap(insertProperties, type, ref value);
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

    private void GenerateSMap(InsertProperties insertProperties, Type type, ref object value)
    {
      if (value is not IDictionary dict)
        return;
      
      var sb = new StringBuilder();

      sb.Append("MAP(");

      bool isFirst = true;

      foreach (DictionaryEntry dictionaryEntry in dict)
      {
        if (isFirst)
          isFirst = false;
        else
          sb.Append(", ");

        var key = ExtractValue(dictionaryEntry.Key, insertProperties, type, dictionaryEntry.Key.GetType());

        sb.Append(key);

        sb.Append(" := ");

        var dictValue = ExtractValue(dictionaryEntry.Value, insertProperties, type, dictionaryEntry.Value.GetType());

        sb.Append(dictValue);
      }

      sb.Append(")");

      value = sb.ToString();
    }

    private void GenerateStruct<T>(InsertProperties insertProperties, Type type, ref object value)
    {
      var sb = new StringBuilder();

      sb.Append("STRUCT(");

      bool isFirst = true;

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
      if(value == null)
        return "NULL";
      
      var enumerableType = type.GetEnumerableTypeDefinition();

      if (enumerableType == null || !enumerableType.Any())
        return value;

      type = enumerableType.First();
      type = type.GetGenericArguments()[0];
        
      var source = ((IEnumerable)value).Cast<object>();
      var array = source.Select(c => ExtractValue(c, insertProperties, null, type)).ToArray();

      if (!array.Any())
        return "ARRAY_REMOVE(ARRAY[0], 0)";

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