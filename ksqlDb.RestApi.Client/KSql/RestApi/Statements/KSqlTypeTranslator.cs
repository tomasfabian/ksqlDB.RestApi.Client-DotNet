using System.Reflection;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements
{
  internal sealed class KSqlTypeTranslator : EntityInfo
  {
    internal static string Translate(Type type, IdentifierEscaping escaping = IdentifierEscaping.Never)
    {
      var ksqlType = string.Empty;

      if (type == typeof(byte[]))
        ksqlType = KSqlTypes.Bytes;
      else if (type.IsArray)
      {
        var elementType = Translate(type.GetElementType(), escaping);

        ksqlType = $"{KSqlTypes.Array}<{elementType}>";
      }
      else if (type.IsDictionary())
      {
        Type[] typeParameters = type.GetGenericArguments();

        var keyType = Translate(typeParameters[0], escaping);
        var valueType = Translate(typeParameters[1], escaping);

        ksqlType = $"{KSqlTypes.Map}<{keyType}, {valueType}>";
      }
      else if (type == typeof(string))
        ksqlType = KSqlTypes.Varchar;
      else if (type == typeof(Guid))
        ksqlType = KSqlTypes.Varchar;
      else if (type.IsOneOfFollowing(typeof(int), typeof(int?), typeof(short), typeof(short?)))
        ksqlType = KSqlTypes.Int;
      else if (type.IsOneOfFollowing(typeof(long), typeof(long?)))
        ksqlType = KSqlTypes.BigInt;
      else if (type.IsOneOfFollowing(typeof(double), typeof(double?)))
        ksqlType = KSqlTypes.Double;
      else if (type.IsOneOfFollowing(typeof(bool), typeof(bool?)))
        ksqlType = KSqlTypes.Boolean;
      else if (type == typeof(decimal))
        ksqlType = KSqlTypes.Decimal;
      else if (type == typeof(DateTime))
        ksqlType = KSqlTypes.Date;
      else if (type == typeof(TimeSpan))
        ksqlType = KSqlTypes.Time;
      else if (type == typeof(DateTimeOffset))
        ksqlType = KSqlTypes.Timestamp;
      else if (!type.IsGenericType && type.TryGetAttribute<StructAttribute>() != null)
      {
        var ksqlProperties = GetProperties(type, escaping);

        ksqlType = $"{KSqlTypes.Struct}<{string.Join(", ", ksqlProperties)}>";
      }
      else if (!type.IsGenericType && (type.IsClass || type.IsStruct()))
      {
        ksqlType = type.Name.ToUpper();
      }
      else if (type.IsEnum)
        ksqlType = KSqlTypes.Varchar;
      else
      {
        Type elementType = null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
          elementType = type.GetGenericArguments()[0];
        else
        {
          var enumerableInterfaces = type.GetEnumerableTypeDefinition().ToArray();

          if (enumerableInterfaces.Length > 0)
          {
            elementType = enumerableInterfaces[0].GetGenericArguments()[0];
          }
        }

        if (elementType != null)
        {
          string ksqlElementType = Translate(elementType, escaping);

          ksqlType = $"ARRAY<{ksqlElementType}>";
        }
      }

      return ksqlType;
    }

    internal static IEnumerable<string> GetProperties(Type type, IdentifierEscaping escaping)
    {
      var ksqlProperties = new List<string>();

      foreach (var memberInfo in Members(type, false))
      {
        var memberType = GetMemberType(memberInfo);

        var ksqlType = Translate(memberType, escaping);

        string columnDefinition = $"{memberInfo.Format(escaping)} {ksqlType}{ExploreAttributes(memberInfo, memberType)}";

        ksqlProperties.Add(columnDefinition);
      }

      return ksqlProperties;
    }

    internal static string ExploreAttributes(MemberInfo memberInfo, Type type)
    {
      if (type == typeof(decimal))
      {
        var decimalMember = memberInfo.TryGetAttribute<DecimalAttribute>();

        if (decimalMember != null)
          return $"({decimalMember.Precision},{decimalMember.Scale})";
      }

      if (type.IsArray)
      {
        var headersAttribute = memberInfo.TryGetAttribute<HeadersAttribute>();

        if (headersAttribute != null)
        {
          const string header = " HEADER";

          if (string.IsNullOrEmpty(headersAttribute.Key))
            return $"{header}S";

          return $"{header}('{headersAttribute.Key}')";
        }
      }

      return string.Empty;
    }
  }
}
