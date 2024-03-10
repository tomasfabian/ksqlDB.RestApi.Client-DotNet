using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.RestApi.Enums;
using ksqlDB.RestApi.Client.KSql.RestApi.Extensions;
using ksqlDb.RestApi.Client.KSql.RestApi.Parsers;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;
using static System.String;
using DateTime = System.DateTime;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal sealed class CreateEntity : CreateEntityStatement
{
  internal static string KSqlTypeTranslator(Type type, IdentifierEscaping escaping = IdentifierEscaping.Never)
  {
    var ksqlType = Empty;

    if (type == typeof(byte[]))
      ksqlType = KSqlTypes.Bytes;
    else if (type.IsArray)
    {
      var elementType = KSqlTypeTranslator(type.GetElementType(), escaping);

      ksqlType = $"ARRAY<{elementType}>";
    }
    else if (type.IsDictionary())
    {
      Type[] typeParameters = type.GetGenericArguments();

      var keyType = KSqlTypeTranslator(typeParameters[0], escaping);
      var valueType = KSqlTypeTranslator(typeParameters[1], escaping);

      ksqlType = $"MAP<{keyType}, {valueType}>";
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

      ksqlType = $"STRUCT<{Join(", ", ksqlProperties)}>";
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
        string ksqlElementType = KSqlTypeTranslator(elementType, escaping);

        ksqlType = $"ARRAY<{ksqlElementType}>";
      }
    }

    return ksqlType;
  }

  private readonly StringBuilder stringBuilder = new();

  internal string Print<T>(StatementContext statementContext, EntityCreationMetadata metadata, bool? ifNotExists)
  {
    stringBuilder.Clear();

    PrintCreateOrReplace<T>(statementContext, metadata);

    if (ifNotExists.HasValue && ifNotExists.Value)
      stringBuilder.Append(" IF NOT EXISTS");

    stringBuilder.Append($"{statementContext.Statement} {statementContext.EntityName}");

    stringBuilder.Append(" (" + Environment.NewLine);

    PrintProperties<T>(statementContext, metadata);

    stringBuilder.Append(')');

    string with = CreateStatements.GenerateWithClause(metadata);

    stringBuilder.Append($"{with};");

    return stringBuilder.ToString();
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

        if (IsNullOrEmpty(headersAttribute.Key))
          return $"{header}S";

        return $"{header}('{headersAttribute.Key}')";
      }
    }

    return Empty;
  }

  private void PrintProperties<T>(StatementContext statementContext, EntityCreationMetadata metadata)
  {
    var ksqlProperties = new List<string>();
    metadata??= new EntityCreationMetadata();
    foreach (var memberInfo in Members<T>(metadata.IncludeReadOnlyProperties))
    {
      var type = GetMemberType(memberInfo);

      var ksqlType = KSqlTypeTranslator(type, metadata.IdentifierEscaping);

      var columnName = IdentifierUtil.Format(memberInfo, metadata.IdentifierEscaping);
      string columnDefinition = $"\t{columnName} {ksqlType}{ExploreAttributes(memberInfo, type)}";

      columnDefinition += TryAttachKey(statementContext.KSqlEntityType, memberInfo);

      ksqlProperties.Add(columnDefinition);
    }

    stringBuilder.AppendLine(Join($",{Environment.NewLine}", ksqlProperties));
  }

  internal static IEnumerable<string> GetProperties(Type type, IdentifierEscaping escaping)
  {
    var ksqlProperties = new List<string>();

    foreach (var memberInfo in Members(type, false))
    {
      var memberType = GetMemberType(memberInfo);

      var ksqlType = KSqlTypeTranslator(memberType, escaping);

      string columnDefinition = $"{memberInfo.Format(escaping)} {ksqlType}{ExploreAttributes(memberInfo, memberType)}";

      ksqlProperties.Add(columnDefinition);
    }

    return ksqlProperties;
  }

  private void PrintCreateOrReplace<T>(StatementContext statementContext, EntityCreationMetadata metadata)
  {
    string creationTypeText = statementContext.CreationType switch
    {
      CreationType.Create => "CREATE",
      CreationType.CreateOrReplace => "CREATE OR REPLACE",
      _ => throw new ArgumentOutOfRangeException(nameof(statementContext), $"Unknown '{nameof(CreationType)}' value {statementContext.CreationType}.")
    };

    string entityTypeText = statementContext.KSqlEntityType switch
    {
      KSqlEntityType.Table => KSqlEntityType.Table.ToString().ToUpper(),
      KSqlEntityType.Stream => KSqlEntityType.Stream.ToString().ToUpper(),
      _ => throw new ArgumentOutOfRangeException(nameof(statementContext), $"Unknown '{nameof(KSqlEntityType)}' value {statementContext.KSqlEntityType}.")
    };

    statementContext.EntityName = EntityProvider.GetFormattedName<T>(metadata);

    string source = metadata.IsReadOnly ? " SOURCE" : Empty;

    stringBuilder.Append($"{creationTypeText}{source} {entityTypeText}");
  }

  private static string TryAttachKey(KSqlEntityType entityType, MemberInfo memberInfo)
  {
    if (!memberInfo.HasKey())
      return Empty;

    string key = entityType switch
    {
      KSqlEntityType.Stream => "KEY",
      KSqlEntityType.Table => "PRIMARY KEY",
      _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
    };

    return $" {key}";
  }
}
