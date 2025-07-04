using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using ksqlDb.RestApi.Client.FluentAPI.Builders;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Formats;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

#nullable disable
internal sealed class CreateKSqlValue(IMetadataProvider metadataProvider) : EntityInfo(metadataProvider)
{
  private const string CREATE_EMPTY_ARRAY = "ARRAY_REMOVE(ARRAY[0], 0)";

  public object ExtractValue<T>(T inputValue, IValueFormatters valueFormatters, MemberInfo memberInfo, Type type, Func<MemberInfo, string> formatter, bool castNulls = false)
  {
    Type valueType = inputValue.GetType();

    object value = inputValue;

    if (memberInfo?.MemberType == MemberTypes.Property && ((PropertyInfo) memberInfo).GetAccessors().Length > 0)
      value = valueType.GetProperty(memberInfo.Name)?.GetValue(inputValue);
    else if (memberInfo?.MemberType == MemberTypes.Field)
      value = valueType.GetField(memberInfo.Name)?.GetValue(inputValue);

    if (value == null)
    {
      if (castNulls && !type.IsArray)
      {
        KSqlTypeTranslator<T> typeTranslator = new(metadataProvider);
        var ksqlType = typeTranslator.Translate(type, memberInfo);
        return $"CAST({KSqlTypes.Null} AS {ksqlType})";
      }
      return KSqlTypes.Null;
    }

    if (type == typeof(decimal))
    {
      Debug.Assert(value != null, nameof(value) + " != null");

      if(valueFormatters.FormatDecimalValue != null)
        value = valueFormatters.FormatDecimalValue((decimal)value);
    }
    else if (type == typeof(TimeSpan))
    {
      TimeSpan timeSpan = (TimeSpan)value;

      value = timeSpan.ToString(ValueFormats.TimeFormat, CultureInfo.InvariantCulture);
      value = $"'{value}'";
    }
    else if (type == typeof(DateTime))
    {
      DateTime date = (DateTime)value;

      value = date.ToString(ValueFormats.DateFormat, CultureInfo.InvariantCulture);
      value = $"'{value}'";
    }
    else if (type == typeof(Guid))
    {
      var guid = ((Guid)value).ToString();

      value = $"'{guid}'";
    }
    else if (type == typeof(DateTimeOffset))
    {
      var dateTimeOffset = (DateTimeOffset)value;

      value = dateTimeOffset.ToString(ValueFormats.DateTimeOffsetFormat, CultureInfo.InvariantCulture);

      value = $"'{value}'";
    }
    else if (type == typeof(double))
    {
      Debug.Assert(value != null, nameof(value) + " != null");

      if(valueFormatters.FormatDoubleValue != null)
        value = valueFormatters.FormatDoubleValue((double)value);
    }
    else if (type == typeof(string))
      value = $"'{value}'";
    else if (type.IsPrimitive)
      value = value.ToString();
    else if (type.IsEnum)
      value = $"'{value}'";
    else if (type.IsDictionary())
      GenerateMap(valueFormatters, type, formatter, ref value);
    else if (type.IsArray)
    {
      value = GenerateEnumerableValue(type, value, valueFormatters, formatter);
    }
    else if (!type.IsGenericType && (type.IsClass || type.IsStruct()))
    {
      GenerateStruct<T>(valueFormatters, type, formatter, ref value);
    }
    else
    {
      value = GenerateEnumerableValue(type, value, valueFormatters, formatter);
    }

    return value;
  }

  private void GenerateMap(IValueFormatters valueFormatters, Type type, Func<MemberInfo, string> formatter,
    ref object value)
  {
    if (value is not IDictionary dict)
      return;

    var sb = new StringBuilder();

    sb.Append($"{KSqlTypes.Map}(");

    bool isFirst = true;

    foreach (DictionaryEntry dictionaryEntry in dict)
    {
      if (isFirst)
        isFirst = false;
      else
        sb.Append(", ");

      var key = ExtractValue(dictionaryEntry.Key, valueFormatters, type, dictionaryEntry.Key.GetType(), formatter);

      sb.Append(key);

      sb.Append(" := ");

      var dictValue = ExtractValue(dictionaryEntry.Value, valueFormatters, type, dictionaryEntry.Value.GetType(), formatter);

      sb.Append(dictValue);
    }

    sb.Append(')');

    value = sb.ToString();
  }

  private void GenerateStruct<T>(IValueFormatters valueFormatters, Type type, Func<MemberInfo, string> formatter,
    ref object value)
  {
    var sb = new StringBuilder();

    sb.Append($"{KSqlTypes.Struct}(");

    bool isFirst = true;

    foreach (var memberInfo2 in Members<T>(type))
    {
      if (isFirst)
        isFirst = false;
      else
        sb.Append(", ");

      type = GetMemberType(memberInfo2);

      var innerValue = ExtractValue(value, valueFormatters, memberInfo2, type, formatter, true);
      var name = formatter(memberInfo2);
      if (type.IsArray && KSqlTypes.Null.Equals(innerValue))
        sb.Append($"{name} := {CREATE_EMPTY_ARRAY}");
      else
        sb.Append($"{name} := {innerValue}");

    }

    sb.Append(')');

    value = sb.ToString();
  }

  private object GenerateEnumerableValue(Type type, object value, IValueFormatters valueFormatters,
    Func<MemberInfo, string> formatter)
  {
    if (value == null)
      return KSqlTypes.Null;

    var enumerableType = type.GetEnumerableTypeDefinition();

    if (enumerableType == null || !enumerableType.Any())
      return value;

    var elementType = type.IsArray ? type.GetElementType() : enumerableType.First();
    elementType = type.IsArray ? elementType : elementType.GetGenericArguments()[0];

    var source = ((IEnumerable)value).Cast<object>();
    var array = source.Select(c => ExtractValue(c, valueFormatters, null, elementType, formatter)).ToArray();

    if (array.Length == 0)
      return CREATE_EMPTY_ARRAY;

    return PrintArray(array);
  }

  private static string PrintArray(object[] array)
  {
    var sb = new StringBuilder();
    sb.Append($"{KSqlTypes.Array}[");
#if NETSTANDARD
    var value = string.Join(", ", array);
#else
    var value = string.Join(", ", array);
#endif
    sb.Append(value);
    sb.Append(']');

    value = sb.ToString();

    return value;
  }
}
